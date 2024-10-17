using EJournalWPF.Data;
using EJournalWPF.Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace EJournalWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage(List<CefSharp.Cookie> cefSharpCookies)
        {
            InitializeComponent();
            DataRepository.Initialize(cefSharpCookies);
            var dataRepository = DataRepository.GetInstance();
            dataRepository.LoadDataSuccessEvent += LoadData;
            dataRepository.UpdateProgressEvent += UpdateDownloadProgress;
            dataRepository.UpdateTextEvent += UpdateDownloadText;
            dataRepository.ResetProgressEvent += ResetDownloadProgress;
        }

        private void LoadData(List<Mail> mails)
        {
            Application.Current.Dispatcher.Invoke(() => EmailListBox.ItemsSource = mails);
        }

        private void UpdateDownloadText(string message)
        {
            Application.Current.Dispatcher.Invoke(() => DownloadTextBlock.Text = message);
        }

        private void UpdateDownloadProgress(int value)
        {
            
            Application.Current.Dispatcher.Invoke(() => DownloadBar.Value += value);
        }

        private void ResetDownloadProgress(int maximum)
        {
            Application.Current.Dispatcher.Invoke(() => {
                DownloadBar.Value = 0;
                DownloadBar.Maximum = maximum;
            });
        }
    }
}
