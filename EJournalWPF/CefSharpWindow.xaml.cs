using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EJournalWPF
{
    /// <summary>
    /// Логика взаимодействия для CefSharpWindow.xaml
    /// </summary>
    public partial class CefSharpWindow : Window
    {
        public List<CefSharp.Cookie> Cookies;


        public CefSharpWindow()
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
                    Cookies = cookies;
                }
                else
                {
                    MessageBox.Show("Увы, что-то не так!");
                }
            });
        }
    }
}
