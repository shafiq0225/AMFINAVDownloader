namespace AMFINAV.Domain.Entities
{
    public class NavFile
    {
        public int Id { get; set; }
        public DateTime NavDate { get; set; }
        public string FileContent { get; set; }
        public int FileSizeBytes { get; set; }
        public DateTime DownloadedAt { get; set; }
        public int RecordCount { get; set; }
        public string Checksum { get; set; }
    }
}