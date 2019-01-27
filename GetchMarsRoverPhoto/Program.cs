using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

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
			int? PictureIndex = null;
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
		}
	}
}
