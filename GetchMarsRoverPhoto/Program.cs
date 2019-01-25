using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

namespace GetchMarsRoverPhoto
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine ("Usage:");
                Console.WriteLine (Path.GetFileName (Environment.GetCommandLineArgs()[0])
                    + " <api-key> <date>");

                return;
            }

            // parameters
            string ApiKey = args[0];
            string strDate = args[1];

            // parse date
            DateTime dtDay = default (DateTime);
            CultureInfo ciEnglish = new CultureInfo ("en-us");
            if (new[]
                {
                    "MM/dd/yy",
                    "MMMM d, yyyy",
                    "MMM-d-yyyy"
                }
                .All (f => !DateTime.TryParseExact (strDate, f, ciEnglish, DateTimeStyles.None, out dtDay))
                )
            {
                Console.WriteLine ("Wrong date format: " + strDate);
                return;
            }

            Console.WriteLine (dtDay.ToString ("yyyy-MM-dd"));
        }
    }
}
