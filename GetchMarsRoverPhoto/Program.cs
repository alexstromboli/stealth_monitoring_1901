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
            if (args.Length < 1)
            {
                Console.Error.WriteLine ("Usage:");
                Console.Error.WriteLine (Path.GetFileName (Environment.GetCommandLineArgs()[0])
                    + " <api-key> [dates-file] [--date <date>]");

                return;
            }

            // parameters
            Utils.Args Args = new Utils.Args (args);
            string strDate = Args.ExtractValue ("--date");

            string ApiKey = Args[0];
            string DatesFilePath = null;

            if (Args.Count > 1)
            {
                DatesFilePath = Path.GetFullPath (Args[1]);
            }

            // parse date
            DateTime? dtDay = Utils.ReadDateTime.Try (strDate);
            if (dtDay == null)
            {
                Console.Error.WriteLine ("Wrong date format: " + strDate);
                return;
            }

            Console.WriteLine (dtDay.Value.ToString ("yyyy-MM-dd"));
        }
    }
}
