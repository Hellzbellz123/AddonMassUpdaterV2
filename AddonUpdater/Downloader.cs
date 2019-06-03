using System;
using System.IO;
using System.Net;

namespace AddonUpdater
{
    public class Downloader
    {
        public static void Filedownload()
        {
            Console.WriteLine(MakeUseableLink.downloadLink);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(MakeUseableLink.downloadLink);
            string filename = "";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result)
            {
                string responseURI = response.ResponseUri.ToString();
                var uri = new Uri(MakeUseableLink.downloadLink);
                filename = Path.GetFileName(responseURI);

                string downDir = MakeUseableLink.downloadDirectory;
                var responseStream = response.GetResponseStream();
                using (var fileStream = File.Create(Path.Combine(downDir + filename)))
                {
                    responseStream.CopyTo(fileStream);
                }
            }
            return;
        }
    }

}