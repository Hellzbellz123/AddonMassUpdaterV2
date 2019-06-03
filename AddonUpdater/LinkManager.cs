/*using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace AddonUpdater
{

    public class MakeUseableLink
    {
        public static string workingDirectory = Directory.GetCurrentDirectory();
        public static string downloadDirectory = workingDirectory + "/Downloads/";
        public static string linkContainer = workingDirectory + "/link.txt";
        public static bool linkExists = File.Exists(linkContainer);
        public static string downloadLink, workingLinkInTextFile, workingLink = null;
        public static List<string> linkList = File.ReadAllLines(linkContainer).ToList();
        public static bool done;
        private static SiteHandler siteHandler = new SiteHandler();
        private static MainWindow window = new MainWindow();

        public void FileCheck()
        {
            if (linkExists == true)
            {
                //new Thread(FormatLinks).Start();
            }
            else
            {
                File.Create(linkContainer);
                window.MessageBoxNoLinks();
            }
        }

        public void FormatLinks()
        {
            foreach (var line in linkList)
            {
                workingLink = line;
                siteHandler.LinkMod();
                if (string.IsNullOrEmpty(workingLink))
                {
                    done = false;
                }
            }
        }

        public void DownloadStuffs()
        {
            if (done != true)
            {
                Console.WriteLine("doing stuff with" + " " + workingLink);
            }
            if (done == true)
            {
                MainWindow.IsDone();
            }
        }
    }
}
*/