using EJournalWPF.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace EJournalWPF.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class Mail
    {
        [JsonProperty("id")]
        public long ID { get; set; }

        [JsonProperty("msg_date")]
        public DateTime Date { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        //[JsonProperty("from_user")]
        public Student FromUser { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("files")]
        public List<File> Files { get; set; }

        [JsonProperty("hasFiles")]
        public bool HasFiles { get; set; }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        public Mail(long id, DateTime date, string subject, Status status, List<File> files, bool hasFiles)
        {
            this.ID = id;
            this.Date = date;
            this.Subject = subject;
            this.Status = status;
            this.Files = files;
            this.HasFiles = true;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            FromUser = DataRepository.GetInstance().GetStudents().Find(s => s.Id == _additionalData["from_user"].ToObject<long>());
        }
    }
}
