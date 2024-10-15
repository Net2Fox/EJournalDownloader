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
