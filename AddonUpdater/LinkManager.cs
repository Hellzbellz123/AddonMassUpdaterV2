using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AddonUpdater
{
    public class MakeUseableLink
    {
        public static string workingDirectory = Directory.GetCurrentDirectory();
        public static string downloadDirectory = workingDirectory + "/downloaded";
        public static string linkContainer = workingDirectory + "/link.txt";
        public static bool linkExists = File.Exists(linkContainer);
        public static string downloadLink, workingLinkInTextFile, workingLink = null;
        public static List<string> linkList = File.ReadAllLines(linkContainer).ToList();
        public static bool done;
        private static SiteHandler siteHandler = new SiteHandler();
        private static MainWindow window = new MainWindow();

        public static void FileCheck()
        {
            if (linkExists == true)
            {
                new Thread(FormatLinks).Start();
            }
            else
            {
                File.Create(linkContainer);
                window.MessageBoxNoLinks();
            }
        }

        public static void FormatLinks()
        {
            foreach (var line in linkList)
            {
                workingLink = line;
                DownloadStuffs();
                SiteHandler.LinkMod();
            }
        }

        public static void DownloadStuffs()
        {
            int WaitTime = 1000;

            if (done != true)
            {
                Console.WriteLine("doing stuff with" + " " + workingLink);
                Console.WriteLine("waiting " + WaitTime + " milliseconds");
                Thread.Sleep(WaitTime);
            }
            if (done == true)
            {
                MainWindow.CurrentLink();
            }

        }
    }
}