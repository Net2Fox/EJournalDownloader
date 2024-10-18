using Newtonsoft.Json;

namespace EJournalWPF.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class File
    {
        [JsonProperty("id")]
        public long ID { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("url")]
        public string URL { get; set; }

        public File(long id, string filename, string url)
        {
            ID = id;
            Filename = filename;
            URL = url;
        }
    }
}
