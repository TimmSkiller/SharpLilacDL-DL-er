using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LilacDL.NET
{
    public class LilacDL
    {
        public string FileName { get; set; }
        public string Uploader { get; set; }
        public int NumberOfParts { get; set; }
        public List<FilePart> FileParts { get; set; }
        public string Size { get; set; }

        public string FullFileMD5 { get; set; }

        public static LilacDL ReadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException();
            }

            List<string> lines = File.ReadAllLines(path).ToList();
            List<FilePart> fileParts = new List<FilePart>();

            string name = lines.Find(c => c.StartsWith("Title:")).Split(':')[1].Trim(); ;
            string uploader = lines.Find(c => c.StartsWith("Uploader:")).Split(':')[1].Trim(); ;
            string size = lines.Find(c => c.StartsWith("Size:")).Split(':')[1].Trim(); ;
            string fullFileMD5 = lines.Find(c => c.StartsWith("MD5:")).Split(':')[1].Trim();

            Regex regex = new Regex("^\\d+:\\shttp");

            List<string> linkLines = lines.Where(line => regex.IsMatch(line)).ToList();

            foreach (string linkLine in linkLines)
            {
                string[] split = linkLine.Split(" ");

                fileParts.Add(new FilePart
                {
                    PartNumber = int.Parse(split[0].Replace(":", "")),
                    URL = split[1],
                    MD5 = split[3],
                    FileName = split[1].Split("/").Last()
                });
            }

            return new LilacDL
            {
                FileName = name,
                Uploader = uploader,
                NumberOfParts = fileParts.Count,
                Size = size,
                FullFileMD5 = fullFileMD5,
                FileParts = fileParts
            };
        }
    }
}