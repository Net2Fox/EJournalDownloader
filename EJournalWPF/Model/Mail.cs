using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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

        [JsonProperty("from_user")]
        public Student FromUser { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("files")]
        public List<File> Files { get; set; }

        [JsonProperty("hasFiles")]
        public bool hasFiles { get; set; }

        public Mail(long id, DateTime date, string subject, Student fromUser, Status status, List<File> files)
        {
            this.ID = id;
            this.Date = date;
            this.Subject = subject;
            this.FromUser = fromUser;
            this.Status = status;
            this.Files = files;
            hasFiles = true;
        }

        public Mail(long id, DateTime date, string subject, Student fromUser, Status status)
        {
            this.ID = id;
            this.Date = date;
            this.Subject = subject;
            this.FromUser = fromUser;
            this.Status = status;
            hasFiles = false;
            Files = null;
        }
    }
}
