namespace LilacDL.NET
{
    public class FilePart
    {
        public int PartNumber { get; set; }
        public string URL { get; set; }
        public string MD5 { get; set; }
        public string FileName { get; set; }

        public override string ToString()
        {
            return $"Part Number: {this.PartNumber}\nLink:        {this.URL}\nMD5:         {this.MD5}\nFile Name:   {this.FileName}";
        }
    }
}