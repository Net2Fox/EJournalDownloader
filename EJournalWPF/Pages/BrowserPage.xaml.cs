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
        List<Group> Groups = new List<Group>();

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
                    //await ProcessMessagesAsync(cookies);
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
                //DownloadWindow downloadWindow = new DownloadWindow(cookies);
                //downloadWindow.Owner = Window.GetWindow(this);
                //downloadWindow.Show();
                //downloadWindow.Focus();
            });
        }

        public async Task ProcessMessagesAsync(List<CefSharp.Cookie> cefSharpCookies)
        {
            CookieContainer cookies = new CookieContainer();

            foreach (var cookie in cefSharpCookies)
            {
                cookies.Add(new Uri("https://kip.eljur.ru"), new System.Net.Cookie(cookie.Name, cookie.Value));
            }
            // https://kip.eljur.ru/journal-api-messages-action?method=messages.get_recipients_list&key1=school&key2=students&key3=2024%2F2025_1_3%D0%98%D0%A1%D0%98%D0%9F-{номер группы}%23%23%23%23%23{id группы}&dep=null
            // Получаем список групп
            JObject recipient_structure = JObject.Parse(await SendRequestAsync("https://kip.eljur.ru/journal-api-messages-action?method=messages.get_recipient_structure", cookies));
            foreach (var structure in recipient_structure["structure"])
            {
                if (structure["key"].ToObject<string>() == "school")
                {
                    foreach (var data in structure["data"])
                    {
                        if (data["key"].ToObject<string>() == "students")
                        {
                            foreach (var group in data["data"])
                            {
                                Groups.Add(new Group(group["name"].ToObject<string>(), group["key"].ToObject<string>().Split(new[] { "#####" }, StringSplitOptions.RemoveEmptyEntries)[1]));
                            }
                        }
                    }
                }
            }
            MessageBox.Show("Получаем список групп.", "Процесс", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);


            // Заполняем в группах студентов
            foreach (var group in Groups)
            {
                JObject user_list = JObject.Parse(await SendRequestAsync($"https://kip.eljur.ru/journal-api-messages-action?method=messages.get_recipients_list&key1=school&key2=students&key3=2024%2F2025_1_{System.Web.HttpUtility.UrlEncode(group.Name)}%23%23%23%23%23{group.Key}&dep=null", cookies));
                foreach (var student in user_list["user_list"])
                {
                    group.Students.Add(new Student(student["id"].ToObject<long>(), student["firstname"].ToObject<string>(), student["lastname"].ToObject<string>(), student["middlename"].ToObject<string>()));
                }
            }
            MessageBox.Show("Получаем список студентов.", "Процесс", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);


            int limit = 10;
            string apiUrl = $"https://kip.eljur.ru/journal-api-messages-action?method=messages.get_list&category=inbox&search=&limit={limit}&offset=0&teacher=21742&status=unread&companion=&minDate=0";
            string jsonResponse = await SendRequestAsync(apiUrl, cookies);
            JObject jsonData = JObject.Parse(jsonResponse);
            MessageBox.Show("Получаем список сообщений.", "Процесс", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            foreach (var message in jsonData["list"])
            {
                if (message["hasFiles"].ToObject<bool>())
                {
                    foreach (var file in message["files"])
                    {
                        string fileUrl = file["url"].ToString();
                        string fileName = file["filename"].ToString();
                        string subDirectory = null;
                        foreach (var group in Groups)
                        {
                            var student = group.Students.Find(s => s.Id == message["from_user"].ToObject<long>());
                            if (student != null)
                            {
                                subDirectory = $"{group.Name}/";
                            }
                        }
                        if (message["files"].Count() > 1)
                        {
                            subDirectory = $"{subDirectory}/{message["subject"].ToObject<string>()}, {message["fromUserHuman"].ToObject<string>()}";
                        }
                        DownloadFile(fileUrl, fileName, subDirectory);
                        //await SendRequestAsync($"https://kip.eljur.ru/journal-api-messages-action?method=messages.note_read&idsString={message["id"]}", cookies);
                    }
                }
            }
            MessageBox.Show("Все файлы успешно скачаны!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
        }

        public async Task<string> SendRequestAsync(string url, CookieContainer cookies)
        {
            using (HttpClientHandler handler = new HttpClientHandler { CookieContainer = cookies, UseCookies = true })
            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36");
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        public void DownloadFile(string fileUrl, string fileName, string subDirectory = null)
        {
            using (WebClient client = new WebClient())
            {
                if (!Directory.Exists("Работа"))
                {
                    Directory.CreateDirectory("Работа");
                }

                if (subDirectory != null)
                {
                    if (!Directory.Exists($"Работа/{subDirectory}"))
                    {
                        Directory.CreateDirectory($"Работа/{subDirectory}");
                    }
                    fileName = $"Работа/{subDirectory}/{fileName}";
                }
                else
                {
                    fileName = $"Работа/{fileName}";
                }

                if (File.Exists(fileName))
                {
                    return;
                }

                byte[] fileBytes = client.DownloadData(fileUrl);

                File.WriteAllBytes(fileName, fileBytes);
            }
        }
    }
}
