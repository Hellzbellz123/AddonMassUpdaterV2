using System;
using System.IO;
using System.Net;

namespace AddonUpdater
{
    public class Downloader
    {
        private static string downloadLink;

        public static void Filedownload()
        {
            downloadLink = SiteHandler.downloadLink;
            Console.WriteLine(downloadLink);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(downloadLink);
            string filename = "";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result)
            {
                string responseURI = response.ResponseUri.ToString();
                var uri = new Uri(downloadLink);
                filename = Path.GetFileName(responseURI);

                string downDir = MakeUseableLink.downloadDirectory;
                var responseStream = response.GetResponseStream();
                Console.WriteLine(downDir);
                using (var fileStream = File.Create(Path.Combine(downDir + filename)))
                {
                    responseStream.CopyTo(fileStream);
                }

            }
            return;
        }
    }

}