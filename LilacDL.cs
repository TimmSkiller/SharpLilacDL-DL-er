using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace friiGameGetterFroamDiscoard 
{
    public class LilacDL 
    {
        public static List<FilePartModel> ReadLilacDL(string path, out LilacDLModel metadata) 
        {
            if (!File.Exists(path)) 
            {
                throw new FileNotFoundException();
            }

            List<string> lines = File.ReadAllLines(path).ToList();
            List<FilePartModel> fileParts = new List<FilePartModel>();

            string name = lines.Find(c => c.StartsWith("Title:"));
            string uploader = lines.Find(c => c.StartsWith("Uploader:"));
            string size = lines.Find(c => c.StartsWith("Size:"));
            string fullFileMD5 = lines.Find(c => c.StartsWith("MD5:")).Split(':')[1].Trim();

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) 
                {
                    continue;
                }

                if (char.IsDigit(line.Split(":")[0][0])) 
                {
                    string[] temp = line.Split(": ");
                    int partNum = int.Parse(temp[0]);
                    string link = temp[1].Split(" * ")[0];
                    string md5 = temp[1].Split(" * ")[1];
                    string[] temp1 = link.Split('/');
                    fileParts.Add(new FilePartModel(partNum, link, md5, temp1[temp1.Length - 1]));
                }
            }

            string numberOfParts = $"Number of Parts: {fileParts.Count}";

            metadata = new LilacDLModel(name, uploader, int.Parse(numberOfParts.Split(':')[1].Trim()), size, fullFileMD5);

            if (!File.Exists($"{Environment.CurrentDirectory}/temp_download")) 
            {
                Directory.CreateDirectory($"{Environment.CurrentDirectory}/temp_download");
            }

            return fileParts;
        }

        public static async Task<Tuple<List<FilePartModel>, List<FilePartModel>>> DownloadPartsParallelAsync(List<FilePartModel> fileParts, IProgress<string> p) 
        {
            if (Directory.Exists($"{Environment.CurrentDirectory}/temp_download")) 
            {
                Directory.Delete($"{Environment.CurrentDirectory}/temp_download", true);
            }

            Stopwatch sw = new Stopwatch();

            WebClient wc = new WebClient();
            List<Task> tasks = new List<Task>();
            List<FilePartModel> notMatchingHashParts = new List<FilePartModel>();
            List<FilePartModel> successfulDownloads = new List<FilePartModel>();
            int progress = 0;
            

            await Task.Run(() => {
                sw.Start();
                Parallel.ForEach<FilePartModel>(fileParts, (filePart) => 
                {
                    Downloader.DownloadPart(filePart.Link, $"{Environment.CurrentDirectory}/temp_download/{filePart.FileName}");
                
                    string hashOfPart = GetMD5($"{Environment.CurrentDirectory}/temp_download/{filePart.FileName}");

                    if(filePart.MD5.ToUpper() != hashOfPart)
                    {
                        notMatchingHashParts.Add(new FilePartModel(filePart.PartNum, filePart.Link, $"{filePart.MD5}|{hashOfPart}", filePart.FileName));
                        p.Report($"{filePart.FileName} failed to download!");
                        return;
                    }

                    progress ++;

                    successfulDownloads.Add(filePart);
                    p.Report($"{filePart.FileName} was successfully downloaded. ({progress}/{fileParts.Count})");
                });
                sw.Stop();
            });

            string hours = sw.Elapsed.Hours.ToString();
            string minutes = sw.Elapsed.Minutes.ToString();
            string seconds = sw.Elapsed.Seconds.ToString();

            if (int.Parse(hours) < 10) 
            {
                hours = $"0{hours}";
            }
            
            if (int.Parse(minutes) < 10) 
            {
                minutes = $"0{minutes}";
            }

            if (int.Parse(seconds) < 10) 
            {
                seconds = $"0{seconds}";
            }

            Console.WriteLine($"Download time: {hours}:{minutes}:{seconds}");

            return Tuple.Create(successfulDownloads, notMatchingHashParts);
        }

        public static void CombineParts(LilacDLModel lilacDL) 
        {
            List<string> partPathsInDirectory = Directory.GetFiles($"{Environment.CurrentDirectory}/temp_download").ToList();
            partPathsInDirectory.Sort();
            List<string> stuffTM = new List<string>();

            if (partPathsInDirectory.Count != lilacDL.NumberOfParts)
            {
                throw new ArgumentException("Amount of parts defined in .lilacDL does not match the amount of files downloaded.");
            }

            FileStream fs = File.Create($"{Environment.CurrentDirectory}/{lilacDL.FullFileName.Split(':')[1].Trim()}");

            Regex r = new Regex(".\\d+$");
            string filePartName = r.Replace(Path.GetFileName(partPathsInDirectory[0]), "");

            for (int i = 0; i < partPathsInDirectory.Count; i++) 
            {
                string path = partPathsInDirectory[i];
                byte[] currentPartByteArr = File.ReadAllBytes($"{Environment.CurrentDirectory}/temp_download/{filePartName}.{i}");
                fs.Write(currentPartByteArr, 0, currentPartByteArr.Length);
            }

            fs.Close();
            Directory.Delete($"{Environment.CurrentDirectory}/temp_download", true);
        }

        public static string Reverse( string s )
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse( charArray );
            return new string( charArray );
        }

        public static string GetMD5(string path) 
        {
            string hashstring = "";
            var md5 = MD5.Create();

            
            byte[] hash = md5.ComputeHash(File.OpenRead(path));
            foreach (byte b in hash)
            {
                hashstring += b.ToString("X2");
            }

            return hashstring;
        }
    }
}