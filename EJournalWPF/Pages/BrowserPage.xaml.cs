using CefSharp;
using EJournalWPF.Model;
using EJournalWPF.Windows;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            };
        }

        public async Task HandleCookiesAsync()
        {
            var cookieManager = Cef.GetGlobalCookieManager();

            var result = await cookieManager.VisitUrlCookiesAsync("https://kip.eljur.ru/", includeHttpOnly: false);
            var cookieCount = result.Count;

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
                NavigationService.Navigate(new MainPage());
            });
        }
    }
}
