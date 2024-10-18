using EJournalWPF.Data;
using EJournalWPF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EJournalWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private bool isDataLoaded = false;
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
            Application.Current.Dispatcher.Invoke(() => {
                EmailListBox.ItemsSource = mails;
                isDataLoaded = true;
            });
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

        private void Filter()
        {
            List<Mail> filteredList = DataRepository.GetInstance().GetMails();

            if (SearchTextBox.Text != String.Empty)
            {
                string text = SearchTextBox.Text.ToLower();
                filteredList = filteredList.Where(m =>
                m.FromUser.FirtsName.ToLower().Contains(text)
                || m.FromUser.LastName.ToLower().Contains(text)
                || m.FromUser.MiddleName.ToLower().Contains(text)
                || m.Subject.ToLower().Contains(text)).ToList();
            }

            if (filteredList.Count != 0 && StatusComboBox.SelectedIndex != 0)
            {
                filteredList = filteredList.Where(m => ((int)m.Status) == StatusComboBox.SelectedIndex).ToList();
            }

            EmailListBox.ItemsSource = filteredList;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isDataLoaded == true)
            {
                Filter();
            }
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isDataLoaded == true)
            {
                Filter();
            }
        }
    }
}
