using EJournalWPF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EJournalWPF
{
    internal class Group
    {
        public string Name { get; set; }
        public string Key { get; set; }

        public Group(string name, string key) 
        {
            this.Name = name;
            this.Key = key;
        }
    }
}
