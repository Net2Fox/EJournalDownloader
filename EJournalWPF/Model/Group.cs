using Newtonsoft.Json;

namespace EJournalWPF
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class Group
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        public Group(string name, string key) 
        {
            this.Name = name;
            this.Key = key;
        }
    }
}
