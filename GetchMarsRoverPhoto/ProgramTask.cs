using System;
using System.IO;

namespace GetchMarsRoverPhoto
{
	// application task parameters, parsed from command line arguments

	class ProgramTask
	{
		// NASA API key
		public string ApiKey { get; protected set; }

		// path to file with dates
		public string DatesFilePath { get; protected set; }

		// specific date
		public DateTime? dtDay { get; protected set; }

		// selected photo index, 1-based
		public int? PhotoIndex { get; protected set; }

		// open the photo in browser
		public bool AutoOpen { get; protected set; }

		// directory to store photos in
		public string OutputDir { get; protected set; }

		public ProgramTask (string[] ArgsRaw)
		{
			// parameters
			Utils.Args Args = new Utils.Args (ArgsRaw);

			// switches
			string strDate = Args.ExtractValue ("--date", "-d");
			string strIndex = Args.ExtractValue ("--index", "-i");
			OutputDir = Path.GetFullPath (Args.ExtractValue ("--outDir") ?? ".");
			AutoOpen = Args.ExtractKey ("--open", "-o") != null;

			// mandatory
			if (ArgsRaw.Length < 1)
			{
				throw new NoArgumentsException ();
			}

			ApiKey = Args[0];
			DatesFilePath = Args.Count > 1 ? Args[1] : null;

			if (DatesFilePath != null)
			{
				DatesFilePath = Path.GetFullPath (Args[1]);
			}

			// either date or file, not both
			if (DatesFilePath != null && strDate != null)
			{
				throw new ArgumentsException ("Cannot specify both date and dates file.");
			}

			// parse date
			dtDay = Utils.ReadDateTime.Try (strDate);
			if (strDate != null && dtDay == null)
			{
				throw new ArgumentsException ("Wrong date format: " + strDate);
			}

			if (dtDay == null && DatesFilePath == null)
			{
				dtDay = DateTime.Today;
			}

			// parse index
			PhotoIndex = null;		// 1-based
			if (strIndex != null)
			{
				int Index;
				if (!int.TryParse (strIndex, out Index))
				{
					throw new ArgumentsException ("Wrong index format: " + strIndex);
				}

				PhotoIndex = Index;
			}

			if (AutoOpen && PhotoIndex == null)
			{
				throw new ArgumentsException ("Auto-open requires photo index.");
			}

			if (PhotoIndex != null && DatesFilePath != null)
			{
				throw new ArgumentsException ("Photo index is only allowed for specific date.");
			}
		}
	}
}
