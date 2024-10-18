using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

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
            this.Key = key.Split(new[] { "#####" }, StringSplitOptions.RemoveEmptyEntries)[1];
        }
    }
}
