namespace EJournalWPF.Model
{
    internal class File
    {
        public long ID { get; set; }
        public string Filename { get; set; }
        public string URL { get; set; }

        public File(long id, string filename, string url)
        {
            ID = id;
            Filename = filename;
            URL = url;
        }
    }
}
