using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace LilacDL.NET
{
    public class Downloader : IDisposable
    {
        private HttpClient Client { get; set; }

        public Downloader()
        {
            this.Client = new HttpClient();
        }

        public async Task<string> DownloadPart(string url, string fileName)
        {
            using (FileStream fs = File.Create(fileName))
            {
                await (await this.Client.GetStreamAsync(url)).CopyToAsync(fs);

                fs.Seek(0, SeekOrigin.Begin);

                return await Tools.GetMD5(fs);
            }
        }

        public void Dispose()
        {
            this.Client.Dispose();
        }
    }
}