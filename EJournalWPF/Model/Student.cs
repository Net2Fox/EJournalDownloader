namespace EJournalWPF.Model
{
    internal class Student
    {
        public long Id { get; set; }
        // Имя
        public string FirtsName { get; set; }
        // Фамилия
        public string LastName { get; set; }
        // Отчество
        public string MiddleName { get; set; }

        public Group Group { get; set; }

        public Student(long id, string firtsName, string lastName, string middleName, Group group)
        {
            this.Id = id;
            this.FirtsName = firtsName;
            this.LastName = lastName;
            this.MiddleName = middleName;
            this.Group = group;
        }
    }
}
