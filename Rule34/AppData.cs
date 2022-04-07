using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rule34
{
    static class AppData
    {
        static string applicationDataFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
        static string lastQueryFile = "LastQuery.txt";
        public static void WriteLastQuery(string lastQuery)
        {
            File.WriteAllText(applicationDataFolder + "\\" + lastQueryFile, lastQuery);
        }

        public static string ReadLastQuery()
        {
            if (File.Exists(applicationDataFolder + "\\" + lastQueryFile))
            {
                return File.ReadAllText(applicationDataFolder + "\\" + lastQueryFile);
            }
            else
                return string.Empty;
        }
    }
}