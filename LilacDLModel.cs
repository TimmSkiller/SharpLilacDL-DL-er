namespace friiGameGetterFroamDiscoard 
{
    public class LilacDLModel
    {
        public string FullFileName {get; set;}
        public string Uploader { get; set; }
        public int NumberOfParts { get; set; }
        public string Size { get; set; }

        public string FullFileMD5 {get; set;}

        public LilacDLModel(string fullFileName, string uploader, int numberOfParts, string size, string fullFileMD5)
        {
            FullFileName = fullFileName;
            Uploader = uploader;
            NumberOfParts = numberOfParts;
            Size = size;
            FullFileMD5 = fullFileMD5;
        }
    }
}