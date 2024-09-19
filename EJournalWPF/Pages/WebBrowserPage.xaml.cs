using CefSharp.Wpf;
using CefSharp;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;

namespace EJournalWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для WebBrowserPage.xaml
    /// </summary>
    public partial class WebBrowserPage : Page
    {
        public WebBrowserPage()
        {
            InitializeComponent();
            browser.FrameLoadEnd += async (sender, e) =>
            {
                if (e.Frame.Url.Contains("https://kip.eljur.ru/?user"))
                {
                    MessageBox.Show("Взлом жопы.", "Хихи", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    await HandleCookiesAsync();
                }
            };
        }

        public async Task HandleCookiesAsync()
        {
            var cookieManager = Cef.GetGlobalCookieManager();

            var result = await cookieManager.VisitUrlCookiesAsync("https://kip.eljur.ru/", includeHttpOnly: false);
            var cookieCount = result.Count;

            await cookieManager.VisitUrlCookiesAsync("https://kip.eljur.ru/", includeHttpOnly: true).ContinueWith(async t =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                {
                    var cookies = t.Result;
                    NavigationService.Navigate(new DownloadPage(cookies));
                    Cef.Shutdown();
                    //await ProcessMessagesAsync(cookies);
                }
                else
                {
                    MessageBox.Show("Увы, что-то не так!");
                }
            });
        }
    }
}
