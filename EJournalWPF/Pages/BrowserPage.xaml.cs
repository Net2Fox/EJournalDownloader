using CefSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace EJournalWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для BrowserPage.xaml
    /// </summary>
    public partial class BrowserPage : Page
    {
        public BrowserPage()
        {
            InitializeComponent();
            browser.FrameLoadEnd += async (sender, e) =>
            {
                if (e.Frame.Url.Contains("https://kip.eljur.ru/?user"))
                {
                    await HandleCookiesAsync();
                }
                else if (e.Frame.Url == "https://kip.eljur.ru/")
                {
                    await HandleCookiesAsync();
                }
            };
        }

        public async Task HandleCookiesAsync()
        {
            var cookieManager = Cef.GetGlobalCookieManager();

            //var result = await cookieManager.VisitUrlCookiesAsync("https://kip.eljur.ru/", includeHttpOnly: false);
            //var cookieCount = result.Count;

            await cookieManager.VisitUrlCookiesAsync("https://kip.eljur.ru/", includeHttpOnly: true).ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                {
                    var cookies = t.Result;
                    ShowWindow(cookies);
                }
                else
                {
                    MessageBox.Show("Увы, что-то не так!");
                }
            });
        }

        public void ShowWindow(List<CefSharp.Cookie> cookies)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                browser.Dispose();
                Cef.Shutdown();
                NavigationService.Navigate(new MainPage(cookies));
            });
        }
    }
}
