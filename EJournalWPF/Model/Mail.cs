using System;
using System.Collections.Generic;

namespace EJournalWPF.Model
{
    internal class Mail
    {
        public long ID { get; set; }
        public DateTime Date { get; set; }
        public string Subject { get; set; }
        public Student FromUser { get; set; }
        public Status Status { get; set; }
        public List<File> Files { get; set; }
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
