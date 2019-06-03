using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace AddonUpdater
{
    public class Downloader
    {
        private static string workingDirectory = Directory.GetCurrentDirectory();
        private static string downloadDirectory = workingDirectory + "/Downloads/";
        private static string linkContainer = workingDirectory + "/link.txt";
        private readonly string curseProject = "://wow.curseforge.com/projects/";
        private readonly string curseForge = "://www.curseforge.com/wow/addons/";
        private readonly string wowAce = "://www.wowace.com/projects/";
        private readonly string wowInterface = "://www.wowinterface.com/";

        private string workingLink, downloadLink = string.Empty;
        readonly HtmlWeb web = new HtmlWeb();
        public static bool done = false;

        public void FormatLinks()
        {
            if (!File.Exists(linkContainer))
                File.Create(linkContainer);
            List<string> linkList = File.ReadAllLines(linkContainer).ToList();
            foreach (var line in linkList)
            {
                workingLink = line;
                if (string.IsNullOrEmpty(workingLink))
                    done = true;
                LinkMod();
            }
        }

        public void LinkMod()
        {
            if (workingLink.Contains(curseProject)) //wow.curseforge.com/projects/
            {
                // handles curseforge.comjustt
                Console.WriteLine("this is a wow.curseforge.com link");
                workingLink += ("/files/latest");
                var htmlDoc = web.Load(workingLink);
                downloadLink = workingLink;
            }
            else if (workingLink.Contains(curseForge) || workingLink.Contains(curseForge)) //curseforge.com/wow/addons/
            {
                // handles curseforge.com
                Console.WriteLine("this is a curseforge.com link");
                if (workingLink.Contains("/download"))
                {
                    var htmlDoc = web.Load(workingLink);
                    foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes("//p/a"))
                    {
                        var hrefValue = node.Attributes["href"]?.Value;
                        downloadLink = $"https://www.curseforge.com{hrefValue}";
                        workingLink = downloadLink;
                    }
                }
                else
                {
                    workingLink += "/download";
                    var htmlDoc = web.Load(workingLink);
                    foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes("//p/a"))
                    {
                        var hrefValue = node.Attributes["href"]?.Value;
                        downloadLink = "https://www.curseforge.com" + hrefValue;
                        workingLink = downloadLink;
                    }
                }
            }
            else if (workingLink.Contains(wowAce)) //www.wowace.com/projects/
            {
                // handles WoWAce
                Console.WriteLine("this is a wowace.com link");
                var htmlDoc = web.Load(workingLink);
                downloadLink = workingLink + "/files/latest";
            }
            else if (workingLink.Contains(wowInterface))  // handles WoWInterface
            {
                var downloadpage = workingLink.Replace("info", "download");
                Console.WriteLine("this is a wowinterface.com link");
                var htmlDoc = web.Load(downloadpage);
                foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes("//div[@class='manuallink']/a"))
                {
                    var hrefValue = node.Attributes["href"]?.Value;
                    downloadLink = hrefValue;
                }
            }
            Filedownload();
        }

        public void DownloadStuffs()
        {
            done = false;
            if (done != true)
            {
                Console.WriteLine("doing stuff with" + " " + workingLink);
                Thread.Sleep(100);
                Filedownload();
            }
            if (done == true)
            {
                MainWindow.IsDone();
            }
        }

        public void Filedownload()
        {
            Console.WriteLine(downloadLink);
            WebRequest request = WebRequest.Create(downloadLink);
            string filename = "";
            using (WebResponse response = (WebResponse)request.GetResponse())
            {
                string responseURI = response.ResponseUri.ToString();
                var uri = new Uri(downloadLink);
                filename = Path.GetFileName(responseURI);

                string downDir = downloadDirectory;
                var responseStream = response.GetResponseStream();
                using (var fileStream = File.Create(Path.Combine(downDir + filename)))
                {
                    responseStream.CopyTo(fileStream);
                }
            }
        }
    }
}