﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Student(long id, string firtsName, string lastName, string middleName)
        {
            this.Id = id;
            this.FirtsName = firtsName;
            this.LastName = lastName;
            this.MiddleName = middleName;
        }
    }
}
