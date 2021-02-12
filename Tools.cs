using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace LilacDL.NET
{
    public static class Tools
    {
        public static async Task<string> GetMD5(Stream s)
        {
            using (MD5 md5 = MD5.Create())
            {
                await md5.ComputeHashAsync(s);

                return md5.Hash.Hex().ToLower();
            }
        }

        public static DirectoryInfo GetTempDir()
        {
            Random random = new Random();

            byte[] bytes = new byte[8];

            random.NextBytes(bytes);

            string dirName = bytes.Hex().ToLower();

            if (!Directory.Exists($"{Path.GetTempPath()}/{dirName}"))
            {
                return Directory.CreateDirectory($"{Path.GetTempPath()}/{dirName}");
            }
            else
            {
                return GetTempDir();
            }
        }

        public static async Task<string> MergeParts(DirectoryInfo partDir, string outputPath)
        {
            List<FileInfo> parts = partDir.GetFiles().ToList();

            parts.Sort((p1, p2) =>
            {
                int ext1 = int.Parse(p1.Extension.Replace(".", ""));
                int ext2 = int.Parse(p2.Extension.Replace(".", ""));

                return ext1.CompareTo(ext2);
            });

            using (FileStream fs = File.Create(outputPath))
            {
                for (int i = 0; i < parts.Count; i++)
                {
                    FileInfo part = parts[i];

                    using (FileStream partfs = part.OpenRead())
                    {
                        await partfs.CopyToAsync(fs);
                        Console.WriteLine($"Merged part #{i + 1}, aka {part.Name}");
                    }
                }

                fs.Seek(0, SeekOrigin.Begin);

                return await GetMD5(fs);
            }
        }

        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }
    }

    public static class Extensions
    {
        public static string Hex(this byte[] bytes)
        {
            string output = "";

            for (int i = 0; i < bytes.Length; i++)
            {
                output += bytes[i].ToString("X2");
            }

            return output;
        }

        public static string ToHumanReadable(this TimeSpan timespan)
        {
            try
            {
                string hours = timespan.Hours.ToString();
                string minutes = timespan.Minutes.ToString();
                string seconds = timespan.Seconds.ToString();

                if (int.Parse(hours) < 10)
                {
                    hours = $"0{hours}h";
                }

                if (int.Parse(minutes) < 10)
                {
                    minutes = $"0{minutes}m";
                }

                if (int.Parse(seconds) < 10)
                {
                    seconds = $"0{seconds}s";
                }

                return $"{hours}{minutes}{seconds}";
            }
            catch
            {
                return "N/A";
            }
        }
    }
}