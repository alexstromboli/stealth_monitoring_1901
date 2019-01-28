using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace GetchMarsRoverPhoto
{
	class Program
	{
		static void PrintUsage ()
		{
				Console.Error.WriteLine ("Usage:");
				Console.Error.WriteLine (Path.GetFileName (Environment.GetCommandLineArgs()[0])
					+ " <api-key> [<dates-file> | --date <date>] [--outDir <output-dir>] [--index <photo-index> [--open]]");
		}

		static void Run (string[] ArgsRaw)
		{
			// task
			ProgramTask ProgramTask = new ProgramTask (ArgsRaw);

			// get the dates
			List<DateTime> Dates = new List<DateTime> ();
			if (ProgramTask.dtDay != null)
			{
				Dates.Add (ProgramTask.dtDay.Value);
			}
			else	// file is already assured to be specified
			{
				string[] DateLines;
				try
				{
					DateLines = File.ReadAllLines (ProgramTask.DatesFilePath);
				}
				catch (FileNotFoundException ex)
				{
					throw new AppException ($"Failed to read {ProgramTask.DatesFilePath}. File must be missing.", ex);
				}
				catch (UnauthorizedAccessException ex)
				{
					throw new AppException ($"Failed to read {ProgramTask.DatesFilePath}. Must be privileges issue.", ex);
				}

				foreach (string DateLine in DateLines)
				{
					DateTime? Date = Utils.ReadDateTime.Try (DateLine);

					if (Date != null)
					{
						Dates.Add (Date.Value);
					}
				}
			}

			if (Dates.Count == 0)
			{
				Console.Error.WriteLine ("No dates specified.");
				return;
			}

			// loading
			WebClient Client = new WebClient ();
			int PhotosTotalCount = 0;
			foreach (DateTime Date in Dates)
			{
				// summary
				string SummaryUrl = $"https://api.nasa.gov/mars-photos/api/v1/rovers/curiosity/photos?earth_date={Date.ToString("yyyy-MM-dd")}&api_key={ProgramTask.ApiKey}";
				
				string SummaryJson;
				try
				{
					SummaryJson = Client.DownloadString (SummaryUrl);
				}
				catch (Exception ex)
				{
					if (ex is WebException wex && wex.Message == "The remote server returned an error: (403) Forbidden.")
					{
						throw new NasaApiForbiddenException (ex);
					}

					throw new NasaApiException ("Failed to download day photos information.", ex);
				}

				// deserialize
				NasaApi.DaySummary DaySummary;
				try
				{
					DaySummary = JsonConvert.DeserializeObject<NasaApi.DaySummary> (SummaryJson);
				}
				catch (Exception ex)
				{
					throw new NasaApiException ("Error while deserializing day photos information. Must be ill-formatted JSON.", ex);
				}

				// verify length
				if (ProgramTask.PhotoIndex != null && DaySummary.Photos.Length < ProgramTask.PhotoIndex.Value)
				{
					Console.Error.WriteLine ($"Photo index ({ProgramTask.PhotoIndex}) exceeds the photos count ({DaySummary.Photos.Length}).");
					return;
				}

				int MinIndex = 0;
				int MaxIndexExcl = DaySummary.Photos.Length;

				if (ProgramTask.PhotoIndex != null)
				{
					MinIndex = ProgramTask.PhotoIndex.Value - 1;		// PhotoIndex is 1-based
					MaxIndexExcl = MinIndex + 1;
				}

				for (int i = MinIndex; i < MaxIndexExcl; ++i)
				{
					NasaApi.Photo Photo = DaySummary.Photos[i];
					string DirPath = Path.Combine (ProgramTask.OutputDir, Photo.EarthDate.ToString ("yyyy-MM-dd"));
					
					try
					{
						Directory.CreateDirectory (DirPath);
					}
					catch (Exception ex)
					{
						throw new AppException ($"Failed to create directory at {DirPath}"
								+ (ex is UnauthorizedAccessException ? ". Must be privileges issue." : ""),
							ex);
					}

					string ImageFileName = $"{Photo.Id}-{Photo.Rover.Name}-{Photo.Camera.Name}.jpg";
					string FilePath = Path.Combine (DirPath, ImageFileName);

					try
					{
						Client.DownloadFile (Photo.ImageUrl, FilePath);
						++PhotosTotalCount;
					}
					catch (Exception ex)
					{
						if (ex is WebException wex)
						{
							if (wex.Message == "The remote server returned an error: (403) Forbidden.")
							{
								throw new NasaApiImageException (Photo.ImageUrl, ex);
							}

							if (wex.InnerException.GetType () == typeof (UnauthorizedAccessException))
							{
								throw new AppException ($"Failed to save photo at {FilePath}. Must be privileges issue.", ex);
							}
						}

						if (ex is DirectoryNotFoundException)
						{
							throw new AppException ($"Failed to save photo at {FilePath}", ex);
						}

						throw new AppException ($"Failed to download photo at {Photo.ImageUrl}. Can be connection fault.", ex);
					}

					if (ProgramTask.AutoOpen)
					{
						Process pOpenImage = new Process ();
						pOpenImage.StartInfo.UseShellExecute = true; 
						pOpenImage.StartInfo.FileName = Photo.ImageUrl;
						pOpenImage.Start();
					}
				}
			}

			if (PhotosTotalCount == 0)
			{
				Console.Error.WriteLine ($"No photos are present for the specified date(s).");
				return;
			}
		}
		static void Main (string[] ArgsRaw)
		{
			try
			{
				Run (ArgsRaw);
			}
			catch (NoArgumentsException)
			{
				PrintUsage ();
			}
			catch (ArgumentsException ex)
			{
				Console.Error.WriteLine (ex.Message);
			}
			catch (Exception ex)
			{
				// display the entire exception stack
				Exception NextEx = ex;

				while (NextEx != null)
				{
					Console.Error.WriteLine (NextEx.GetType ().FullName + ": " + NextEx.Message);
					Console.Error.WriteLine (NextEx.StackTrace);

					NextEx = NextEx.InnerException;

					if (NextEx != null)
					{
						Console.Error.WriteLine ();
					}
				}
			}
		}
	}
}
