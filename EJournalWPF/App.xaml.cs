using CefSharp.Wpf;
using CefSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace EJournalWPF
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var settings = new CefSettings();
            
            settings.LogSeverity = LogSeverity.Verbose;
            settings.CachePath = Path.Combine(Environment.CurrentDirectory, "CefSharp\\Cache");
            
            Cef.Initialize(settings);
        }
    }
}
