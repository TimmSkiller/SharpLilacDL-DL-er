using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using CommandLine;
using System.Diagnostics;
using System.Linq;

namespace LilacDL.NET
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<Options>(args)
                .MapResult(
                    (Options options) => DownloadLilacDLFile(options), 
                    errs => Task.FromResult(0)
                );
        }

        public static async Task DownloadLilacDLFile(Options options)
        {
            DirectoryInfo temp_dir = Tools.GetTempDir();
            Console.WriteLine($"Using temporary directory: {temp_dir.FullName}");
            Stopwatch sw = new Stopwatch();
            LilacDL dl = LilacDL.ReadFromFile(options.LilacDLPath);

            try
            {
                Downloader downloader = new Downloader();

                List<FilePart> fails = new List<FilePart>();

                Console.WriteLine("Starting download...");

                sw.Start();

                List<List<FilePart>> batches = Tools.SplitList(dl.FileParts, options.MaxParallelDownloads).ToList();

                List<Task> batchTasks = new List<Task>();

                foreach (List<FilePart> batch in batches)
                {
                    foreach (FilePart part in batch)
                    {
                        batchTasks.Add(DownloadPart(downloader, part, temp_dir, fails));
                    }

                    await Task.WhenAll(batchTasks);

                    batchTasks.Clear();
                }

                int retries = 0;

                while (fails.Count > 0)
                {
                    if (retries < options.MaxRetries)
                    {
                        retries++;
                    }
                    else
                    {
                        if (fails.Count > 0)
                        {
                            Console.WriteLine($"ERROR: Maximum retries reached ({options.MaxRetries}).");
                            downloader.Dispose();
                            Environment.Exit(-1);
                        }
                    }

                    List<FilePart> redownloadedParts = new List<FilePart>();

                    foreach (FilePart part in fails)
                    {
                        try
                        {
                            Console.WriteLine($"Attempting to redownload part #{part.PartNumber} (Retry #{retries})");

                            await DownloadPart(downloader, part, temp_dir, fails);
                        }
                        catch
                        {
                            break;
                        }

                        redownloadedParts.Add(part);
                    }

                    foreach (FilePart part in redownloadedParts)
                    {
                        fails.Remove(part);
                    }
                }

                sw.Stop();

                Console.WriteLine($"Finished downloading parts. Elapsed time: {sw.Elapsed.ToHumanReadable()}");

                downloader.Dispose();
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The specified file \"{e.FileName}\" was not found.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
                Console.WriteLine($"Stack Trace: {e.StackTrace}");

                temp_dir.Delete(true);
            }

            Console.WriteLine("Merging parts...");

            sw.Reset();
            sw.Start();

            string hash = await Tools.MergeParts(temp_dir, $"./{dl.FileName}");

            Console.WriteLine(hash == dl.FullFileMD5);
            Console.WriteLine($"{hash} OG: {dl.FullFileMD5}");

            sw.Stop();

            temp_dir.Delete(true);

            Console.WriteLine($"Merge complete in {sw.Elapsed.ToHumanReadable()}");
        }

        public static async Task DownloadPart(Downloader downloader, FilePart part, DirectoryInfo downloadDir, List<FilePart> fails)
        {
            try
            {
                string hash = await downloader.DownloadPart(part.URL, $"{downloadDir.FullName}/{part.FileName}");

                if (!(hash == part.MD5))
                {
                    Console.WriteLine($"Part #{part.PartNumber} got wrong hash, expected {part.MD5} but got {hash}");
                    fails.Add(part);
                }

                Console.WriteLine($"Successfully downloaded part #{part.PartNumber}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to download part #{part.PartNumber}!");
                Console.WriteLine($"Exception: {e.Message}");
                Console.WriteLine($"Stack Trace: {e.StackTrace}");

                if (!fails.Contains(part))
                    fails.Add(part);
            }
        }
    }

    public class Options
    {
        [Option('l', "lilacdl-file", HelpText = "The LilacDL file to use.", Required = true)]
        public string LilacDLPath { get; set; }

        [Option('r', "max-retries", HelpText = "The maximum amount of retries to download parts. Defaults to 3.", Required = false)]
        public int MaxRetries { get; set; } = 3;

        [Option('d', "max-parallel-dls", HelpText = "The maximum amount of simultaneous downloads. Defaults to 20.", Required = false)]
        public int MaxParallelDownloads { get; set; } = 20;
    }
}
