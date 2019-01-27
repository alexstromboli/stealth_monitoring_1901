using System.Linq;
using System.Collections.Generic;

namespace GetchMarsRoverPhoto.Utils
{
	class Args : List<string>
	{
		public Args(string[] RawArgs)
			: base (RawArgs)
		{
		}

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

		public string ExtractValue (string Key)
		{
			return ExtractValue (new[] { Key });
		}
	}
}
