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
        private int limit = 20;
        private int offset = 0;
        private DataRepository repository;

        public MainPage(List<CefSharp.Cookie> cefSharpCookies)
        {
            InitializeComponent();
            DataRepository.Initialize(cefSharpCookies);
            repository = DataRepository.GetInstance();
            repository.LoadDataSuccessEvent += LoadData;
            repository.BeginDataLoadingEvent += DataLoadingProgress;
            repository.DataLoadingErrorEvent += DataLoadingErrorEvent;
        }

        private void DataLoadingErrorEvent(string errorMsg)
        {
            Application.Current.Dispatcher.Invoke(() => {
                LoadingSplashPanel.Visibility = Visibility.Visible;
                LoadingTextBlock.Text = errorMsg;
            });
        }

        private void LoadData(List<Mail> mails)
        {
            Application.Current.Dispatcher.Invoke(() => {
                EmailListBox.ItemsSource = mails;
                Filter();
                isDataLoaded = true;
                LoadingSplashPanel.Visibility = Visibility.Collapsed;
            });
        }

        private void DataLoadingProgress()
        {
            Application.Current.Dispatcher.Invoke(() => {
                LoadingTextBlock.Text = "Загрузка данных, пожалуйста, подождите...";
                LoadingSplashPanel.Visibility = Visibility.Visible;
            });
        }

        private void Filter()
        {
            List<Mail> filteredList = repository.GetMails();

            if (SearchTextBox.Text != string.Empty && SearchTextBox.Text != "Поиск")
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Перемещаться назад offset-limit(n) пока offset != 0
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Перемещаться вперёд, offset+limit(n)
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "Поиск")
            {
                SearchTextBox.Text = string.Empty;
            }
        }

        private void CountTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (CountTextBox.Text == "Количество писем (по умолчанию 20)")
            {
                CountTextBox.Text = string.Empty;
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == string.Empty)
            {
                SearchTextBox.Text = "Поиск";
            }
        }

        private void CountTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CountTextBox.Text == string.Empty)
            {
                CountTextBox.Text = "Количество писем (по умолчанию 20)";
            }
        }

        private async void CountTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && isDataLoaded && int.TryParse(CountTextBox.Text, out limit))
            {
                await repository.GetMailsFromAPI(limit);
            }
        }
    }
}
