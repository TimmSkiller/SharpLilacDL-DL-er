namespace friiGameGetterFroamDiscoard 
{
    public class FilePartModel 
    {
        public int PartNum { get; set; }
        public string Link { get; set; }
        public string MD5 { get; set; }
        public string FileName { get; set; }

        public FilePartModel(int partnum, string link, string md5, string fileName)
        {
            PartNum = partnum;
            Link = link;
            MD5 = md5;
            FileName = fileName;
        }
    }
}