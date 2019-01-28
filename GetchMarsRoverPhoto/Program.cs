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
					+ " <api-key> [<dates-file> | --date <date>] [--outDir <output-dir>] [--index <pic-index> [--open]]");
		}

		static void Main(string[] args)
		{
			// parameters
			Utils.Args Args = new Utils.Args (args);

			// switches
			string strDate = Args.ExtractValue ("--date", "-d");
			string strIndex = Args.ExtractValue ("--index", "-i");
			string OutputDir = Path.GetFullPath (Args.ExtractValue ("--outDir") ?? ".");
			bool AutoOpen = Args.ExtractKey ("--open", "-o") != null;

			// mandatory
			if (args.Length < 1)
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
			int? PictureIndex = null;		// 1-based
			if (strIndex != null)
			{
				int Index;
				if (!int.TryParse (strIndex, out Index))
				{
					Console.Error.WriteLine ("Wrong index format: " + strIndex);
					return;
				}

				PictureIndex = Index;
			}

			if (AutoOpen && PictureIndex == null)
			{
				Console.Error.WriteLine ("Auto-open requires picture index.");
				return;
			}

			if (PictureIndex != null && DatesFilePath != null)
			{
				Console.Error.WriteLine ("Picture index is only allowed for specific date.");
				return;
			}

			// get the dates
			List<DateTime> Dates = new List<DateTime> ();
			if (dtDay != null)
			{
				Dates.Add (dtDay.Value);
			}
			else	// file is already assured to be given
			{
				// here: check that file exists
				string[] DateLines = File.ReadAllLines (DatesFilePath);
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
			foreach (DateTime Date in Dates)
			{
				// summary
				string SummaryUrl = $"https://api.nasa.gov/mars-photos/api/v1/rovers/curiosity/photos?earth_date={Date.ToString("yyyy-MM-dd")}&api_key={ApiKey}";
				string SummaryJson = Client.DownloadString (SummaryUrl);
				NasaApi.DaySummary DaySummary = JsonConvert.DeserializeObject<NasaApi.DaySummary> (SummaryJson);

				if (PictureIndex != null && DaySummary.Photos.Length < PictureIndex.Value)
				{
					Console.Error.WriteLine ($"Picture index ({PictureIndex}) exceeds the photos count ({DaySummary.Photos.Length}).");
					return;
				}

				int MinIndex = 0;
				int MaxIndexExcl = DaySummary.Photos.Length;

				if (PictureIndex != null)
				{
					MinIndex = PictureIndex.Value - 1;		// PictureIndex is 1-based
					MaxIndexExcl = MinIndex + 1;
				}

				for (int i = MinIndex; i < MaxIndexExcl; ++i)
				{
					NasaApi.Photo Photo = DaySummary.Photos[i];
					string DirPath = Path.Combine (OutputDir, Photo.EarthDate.ToString ("yyyy-MM-dd"));
					Directory.CreateDirectory (DirPath);

					string Extension = Path.GetExtension (Photo.ImageUrl);
					string ImageFileName = $"{Photo.Id}-{Photo.Rover.Name}-{Photo.Camera.Name}" + Extension;
					string FilePath = Path.Combine (DirPath, ImageFileName);

					// here: capture failure
					Client.DownloadFile (Photo.ImageUrl, FilePath);

					if (AutoOpen)
					{
						Process pOpenImage = new Process ();
						pOpenImage.StartInfo.UseShellExecute = true; 
						pOpenImage.StartInfo.FileName = Photo.ImageUrl;
						pOpenImage.Start();
					}
				}
			}
		}
	}
}
