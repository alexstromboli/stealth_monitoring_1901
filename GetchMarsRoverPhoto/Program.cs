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
					+ " <api-key> [<dates-file> | --date <date>] [--index <pic-index> [--open]]");
		}

		static void Main(string[] args)
		{
			// parameters
			Utils.Args Args = new Utils.Args (args);

			// switches
			string strDate = Args.ExtractValue ("--date", "-d");
			string strIndex = Args.ExtractValue ("--index", "-i");
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
			if (dtDay == null)
			{
				Console.Error.WriteLine ("Wrong date format: " + strDate);
				return;
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

			Console.WriteLine (dtDay.Value.ToString ("yyyy-MM-dd"));
		}
	}
}
