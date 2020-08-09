using System.Threading.Tasks;
using System.Net;
namespace friiGameGetterFroamDiscoard 
{
    public static class Downloader 
    {
        public static void DownloadPart(string url, string fileName) 
        {
            WebClient wc = new WebClient();
            wc.DownloadFile(url, fileName);
        }
    }
}