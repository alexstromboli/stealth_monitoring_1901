using System;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GetchMarsRoverPhoto.Utils
{
	class ReadDateTime
	{
		public static readonly CultureInfo CultureInfo = new CultureInfo ("en-us");

		public static readonly string[] Formats = new[]
				{
					"MM/dd/yy",
					"MMMM d, yyyy",
					"MMM-d-yyyy"
				};

		public static DateTime? Try (string Input)
		{
			if (Input == null)
			{
				return null;
			}

			Input = Regex.Replace (Input, @"\s+", " ").Trim ();

            DateTime dtDay = default (DateTime);

            if (Formats.All (f => !DateTime.TryParseExact (Input, f, CultureInfo, DateTimeStyles.None, out dtDay)))
            {
                return null;
            }

			return dtDay;
		}
	}
}
