using System.Windows;
using System.Windows.Controls;

namespace EJournalWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
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
