using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace friiGameGetterFroamDiscoard
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try 
            {
                if (File.Exists(args[0]) && args.Length == 1) 
                {
                    List<FilePartModel> parts = LilacDL.ReadLilacDL(args[0], out LilacDLModel metadata);

                    Console.WriteLine($"{metadata.FullFileName}\n{metadata.Size}\nNumber of Parts: {metadata.NumberOfParts}\n{metadata.Uploader}\n");

                    var results = await LilacDL.DownloadPartsParallelAsync(parts, new Progress<string>(c => Console.WriteLine(c)));

                    if (results.Item2.Count > 0) 
                    {
                        Console.WriteLine("Retrying failed downloads...");

                        foreach(FilePartModel fpm in results.Item2) 
                        {
                            var retriedResults = await LilacDL.DownloadPartsParallelAsync(results.Item2, new Progress<string>(c => {Console.WriteLine(c);}));

                            if (retriedResults.Item2.Count > 0) 
                            {
                                Console.WriteLine("Even after retrying the download of the failed parts, the downloads failed again. There's a high chance the uploaded parts are invalid.");
                            }
                        }
                    }

                    LilacDL.CombineParts(metadata);
                }
                else 
                {
                    Console.WriteLine($"The Specified file does not exist.\nUsage: {System.AppDomain.CurrentDomain.FriendlyName} PATH_TO_LILACDL");
                }

            }
            catch (IndexOutOfRangeException) 
            {
                Console.WriteLine($"Invalid Parameters.\nUsage: {System.AppDomain.CurrentDomain.FriendlyName} PATH_TO_LILACDL");
            }
            catch (ArgumentException e) 
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
