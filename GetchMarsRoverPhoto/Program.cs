using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
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
			// parameters
			Utils.Args Args = new Utils.Args (ArgsRaw);

			// switches
			string strDate = Args.ExtractValue ("--date", "-d");
			string strIndex = Args.ExtractValue ("--index", "-i");
			string OutputDir = Path.GetFullPath (Args.ExtractValue ("--outDir") ?? ".");
			bool AutoOpen = Args.ExtractKey ("--open", "-o") != null;

			// mandatory
			if (ArgsRaw.Length < 1)
			{
				PrintUsage ();
				return;
			}

			string ApiKey = Args[0];
			string DatesFilePath = Args.Count > 1 ? Args[1] : null;

			if (DatesFilePath != null)
			{
				DatesFilePath = Path.GetFullPath (Args[1]);
			}

			// either date or file, not both
			if (DatesFilePath != null && strDate != null)
			{
				PrintUsage ();
				return;
			}

			// parse date
			DateTime? dtDay = Utils.ReadDateTime.Try (strDate);
			if (strDate != null && dtDay == null)
			{
				Console.Error.WriteLine ("Wrong date format: " + strDate);
				return;
			}

			if (dtDay == null && DatesFilePath == null)
			{
				dtDay = DateTime.Today;
			}

			// parse index
			int? PhotoIndex = null;		// 1-based
			if (strIndex != null)
			{
				int Index;
				if (!int.TryParse (strIndex, out Index))
				{
					Console.Error.WriteLine ("Wrong index format: " + strIndex);
					return;
				}

				PhotoIndex = Index;
			}

			if (AutoOpen && PhotoIndex == null)
			{
				Console.Error.WriteLine ("Auto-open requires photo index.");
				return;
			}

			if (PhotoIndex != null && DatesFilePath != null)
			{
				Console.Error.WriteLine ("Photo index is only allowed for specific date.");
				return;
			}

			// get the dates
			List<DateTime> Dates = new List<DateTime> ();
			if (dtDay != null)
			{
				Dates.Add (dtDay.Value);
			}
			else	// file is already assured to be specified
			{
				string[] DateLines;
				try
				{
					DateLines = File.ReadAllLines (DatesFilePath);
				}
				catch (FileNotFoundException ex)
				{
					throw new AppException ($"Failed to read {DatesFilePath}. File must be missing.", ex);
				}
				catch (UnauthorizedAccessException ex)
				{
					throw new AppException ($"Failed to read {DatesFilePath}. Must be privileges issue.", ex);
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
				string SummaryUrl = $"https://api.nasa.gov/mars-photos/api/v1/rovers/curiosity/photos?earth_date={Date.ToString("yyyy-MM-dd")}&api_key={ApiKey}";
				
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
				if (PhotoIndex != null && DaySummary.Photos.Length < PhotoIndex.Value)
				{
					Console.Error.WriteLine ($"Photo index ({PhotoIndex}) exceeds the photos count ({DaySummary.Photos.Length}).");
					return;
				}

				int MinIndex = 0;
				int MaxIndexExcl = DaySummary.Photos.Length;

				if (PhotoIndex != null)
				{
					MinIndex = PhotoIndex.Value - 1;		// PhotoIndex is 1-based
					MaxIndexExcl = MinIndex + 1;
				}

				for (int i = MinIndex; i < MaxIndexExcl; ++i)
				{
					NasaApi.Photo Photo = DaySummary.Photos[i];
					string DirPath = Path.Combine (OutputDir, Photo.EarthDate.ToString ("yyyy-MM-dd"));
					
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

					if (AutoOpen)
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
		static void Main (string[] args)
		{
			try
			{
				Run (args);
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
