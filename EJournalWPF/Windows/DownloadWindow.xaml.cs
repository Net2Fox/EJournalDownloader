using EJournalWPF.Model;
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
using System.Windows.Shapes;

namespace EJournalWPF.Windows
{
    /// <summary>
    /// Логика взаимодействия для DownloadWindow.xaml
    /// </summary>
    public partial class DownloadWindow : Window
    {
        List<Group> Groups = new List<Group>();

        public DownloadWindow(List<CefSharp.Cookie> cookies = null)
        {
            InitializeComponent();
            if (cookies != null)
            {
                Task.Run(async () =>
                {
                    await ProcessMessagesAsync(cookies);
                });
            }
        }

        public async Task ProcessMessagesAsync(List<CefSharp.Cookie> cefSharpCookies)
        {
            CookieContainer cookies = new CookieContainer();
            Application.Current.Dispatcher.Invoke(() =>
            {
                DownloadTextBlock.Text = "Загружаем куки...";

            });
            
            foreach (var cookie in cefSharpCookies)
            {
                cookies.Add(new Uri("https://kip.eljur.ru"), new System.Net.Cookie(cookie.Name, cookie.Value));
            }

            // Получаем список групп
            JObject recipient_structure = JObject.Parse(await SendRequestAsync("https://kip.eljur.ru/journal-api-messages-action?method=messages.get_recipient_structure", cookies));
            Application.Current.Dispatcher.Invoke(() =>
            {
                DownloadTextBlock.Text = "Получаем список групп...";
                DownloadBar.Maximum = recipient_structure["structure"].Count();
            });
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
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    DownloadBar.Value += 1;
                                });
                                
                            }
                        }
                    }
                }
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                DownloadTextBlock.Text = "Получаем список студентов...";
                DownloadBar.Value = 0;
            });
            
            // Заполняем в группах студентов
            foreach (var group in Groups)
            {
                JObject user_list = JObject.Parse(await SendRequestAsync($"https://kip.eljur.ru/journal-api-messages-action?method=messages.get_recipients_list&key1=school&key2=students&key3=2024%2F2025_1_{System.Web.HttpUtility.UrlEncode(group.Name)}%23%23%23%23%23{group.Key}&dep=null", cookies));
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DownloadBar.Value = 0;
                    DownloadBar.Maximum = user_list["user_list"].Count();
                });
                
                foreach (var student in user_list["user_list"])
                {
                    group.Students.Add(new Student(student["id"].ToObject<long>(), student["firstname"].ToObject<string>(), student["lastname"].ToObject<string>(), student["middlename"].ToObject<string>()));
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DownloadBar.Value += 1;
                    });
                    
                }
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                DownloadTextBlock.Text = "Получаем список сообщений...";
                DownloadBar.Value = 0;
            });
            
            int limit = 10;
            string apiUrl = $"https://kip.eljur.ru/journal-api-messages-action?method=messages.get_list&category=inbox&search=&limit={limit}&offset=0&teacher=21742&status=unread&companion=&minDate=0";
            string jsonResponse = await SendRequestAsync(apiUrl, cookies);
            JObject jsonData = JObject.Parse(jsonResponse);
            Application.Current.Dispatcher.Invoke(() =>
            {
                DownloadBar.Maximum = jsonData["list"].Count();

            });
            foreach (var message in jsonData["list"])
            {
                if (message["hasFiles"].ToObject<bool>())
                {
                    foreach (var file in message["files"])
                    {
                        string fileUrl = file["url"].ToString();
                        string fileName = file["filename"].ToString();
                        string subDirectory = null;
                        var group = Groups.Find(g => g.Students.Exists(s => s.Id == message["from_user"].ToObject<long>()));
                        var student = group.Students.Find(s => s.Id == message["from_user"].ToObject<long>());
                        if (student != null)
                        {
                            subDirectory = $"{group.Name}/";
                        }
                        //foreach (var group in Groups)
                        //{
                        //    var student = group.Students.Find(s => s.Id == message["from_user"].ToObject<long>());
                        //    if (student != null)
                        //    {
                        //        subDirectory = $"{group.Name}/";
                        //    }
                        //}
                        if (message["files"].Count() > 1)
                        {
                            subDirectory = $"{subDirectory}/{message["subject"].ToObject<string>()}, {message["fromUserHuman"].ToObject<string>()}";
                        }
                        DownloadFile(fileUrl, fileName, subDirectory);
                        //await SendRequestAsync($"https://kip.eljur.ru/journal-api-messages-action?method=messages.note_read&idsString={message["id"]}", cookies);
                    }
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DownloadBar.Value += 1;

                });
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                DownloadTextBlock.Text = "Все файлы успешно скачаны!";

            });
            //MessageBox.Show("Все файлы успешно скачаны!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
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
                    //Console.WriteLine($"Файл '{fileName}' уже скачан, пропускаем...");
                    return;
                }

                byte[] fileBytes = client.DownloadData(fileUrl);

                File.WriteAllBytes(fileName, fileBytes);
                //Console.WriteLine($"Файл '{fileName}' успешно скачан.");
            }
        }
    }
}
