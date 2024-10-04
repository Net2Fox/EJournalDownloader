using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
