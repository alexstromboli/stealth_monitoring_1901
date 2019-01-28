using System.Linq;
using System.Collections.Generic;

// parsing command line arguments

namespace GetchMarsRoverPhoto.Utils
{
	class Args : List<string>
	{
		public Args(string[] RawArgs)
			: base (RawArgs)
		{
		}

		// find and extract a value specified by command line key
		public string ExtractValue (params string[] Keys)
		{
			for (int i = 0; i < Count - 1; ++i)
			{
				if (Keys.Any (k => k == this[i]))
				{
					string Result = this[i + 1];
					RemoveAt(i + 1);
					RemoveAt(i);
					return Result;
				}
			}

			return null;
		}

		// find and extract a command line switch
		public string ExtractKey (params string[] Keys)
		{
			for (int i = 0; i < Count; ++i)
			{
				if (Keys.Any (k => k == this[i]))
				{
					string Result = this[i];
					RemoveAt(i);
					return Result;
				}
			}

			return null;
		}
	}
}
