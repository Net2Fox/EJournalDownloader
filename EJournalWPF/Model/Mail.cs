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
        public bool Readed { get; set; }
        public List<File> Files { get; set; }
        public bool hasFiles { get; set; }

    }
}
