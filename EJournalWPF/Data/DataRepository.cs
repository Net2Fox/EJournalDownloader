using EJournalWPF.Model;
using EJournalWPF.Pages;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EJournalWPF.Data
{
    internal class DataRepository
    {
        private static DataRepository _instance;
        private static readonly object _lock = new object();

        private readonly CookieContainer _cookies = new CookieContainer();

        private readonly List<Group> _groups = new List<Group> { };
        private readonly List<Student> _students = new List<Student> { };
        private readonly List<Mail> _mails = new List<Mail> { };

        internal delegate void LoadDataSuccessHandler(List<Mail> mails);
        internal event LoadDataSuccessHandler LoadDataSuccessEvent;

        internal delegate void UpdateTextHandler(string message);
        internal event UpdateTextHandler UpdateTextEvent;

        internal delegate void UpdateProgressHandler(int prgoress);
        internal event UpdateProgressHandler UpdateProgressEvent;

        internal delegate void ResetProgressHandler(int maximum);
        internal event ResetProgressHandler ResetProgressEvent;

        private DataRepository(List<CefSharp.Cookie> cefSharpCookies)
        {
            foreach (var cookie in cefSharpCookies)
            {
                _cookies.Add(new Uri("https://kip.eljur.ru"), new System.Net.Cookie(cookie.Name, cookie.Value));
            }

            Task.Run(async () =>
            {
                await GetStudentsFromAPI();
                await GetMailsFromAPI();
            });
        }

        internal async Task GetStudentsFromAPI()
        {
            _groups.Clear();
            // Получаем список групп
            UpdateTextEvent?.Invoke("Получаем список групп...");
            JObject recipient_structure = JObject.Parse(await SendRequestAsync("https://kip.eljur.ru/journal-api-messages-action?method=messages.get_recipient_structure", _cookies));
            ResetProgressEvent?.Invoke(recipient_structure["structure"].Count());

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
                                _groups.Add(new Group(group["name"].ToObject<string>(), group["key"].ToObject<string>().Split(new[] { "#####" }, StringSplitOptions.RemoveEmptyEntries)[1]));
                                UpdateProgressEvent?.Invoke(1);
                            }
                        }
                    }
                }
            }
            UpdateTextEvent?.Invoke("Получаем список студентов...");

            // Получаем список студентов
            _students.Clear();
            var studentTasks = _groups.Select(async group =>
            {
                JObject user_list = JObject.Parse(await SendRequestAsync($"https://kip.eljur.ru/journal-api-messages-action?method=messages.get_recipients_list&key1=school&key2=students&key3=2024%2F2025_1_{System.Web.HttpUtility.UrlEncode(group.Name)}%23%23%23%23%23{group.Key}&dep=null", _cookies));

                ResetProgressEvent?.Invoke(user_list["user_list"].Count());

                foreach (var student in user_list["user_list"])
                {
                    _students.Add(new Student(student["id"].ToObject<long>(), student["firstname"].ToObject<string>(), student["lastname"].ToObject<string>(), student["middlename"].ToObject<string>(), group));
                    UpdateProgressEvent?.Invoke(1);
                }
            });

            // Ждем завершения всех заданий
            await Task.WhenAll(studentTasks);
        }

        internal List<Student> GetStudents()
        {
            return _students;
        }

        internal async Task GetMailsFromAPI(int limit = 20, int offset = 0, Status status = Status.all)
        {
            UpdateTextEvent?.Invoke("Получаем список сообщений...");
            _mails.Clear();
            string apiUrl = $"https://kip.eljur.ru/journal-api-messages-action?method=messages.get_list&category=inbox&search=&limit={limit}&offset={offset}&teacher=21742&status={(status == Status.all ? "" : status.ToString())}&companion=&minDate=0";
            string jsonResponse = await SendRequestAsync(apiUrl, _cookies);
            JObject jsonData = JObject.Parse(jsonResponse);
            ResetProgressEvent?.Invoke(jsonData["list"].Count());
            foreach (var message in jsonData["list"])
            {
                long fromUserId = message["from_user"].ToObject<long>();
                if (message["hasFiles"].ToObject<bool>() == true)
                {
                    List<Model.File> files = new List<Model.File>();
                    foreach (var file in message["files"])
                    {
                        files.Add(new Model.File
                        (
                            file["id"].ToObject<long>(),
                            file["filename"].ToObject<string>(),
                            file["url"].ToObject<string>()
                        ));
                    }
                    _mails.Add(new Mail
                    (
                        message["id"].ToObject<long>(),
                        message["msg_date"].ToObject<DateTime>(),
                        message["subject"].ToObject<string>(),
                        _students.Find(s => s.Id == fromUserId),
                        message["status"].ToObject<Status>(),
                        files
                    ));
                }
                else
                {
                    _mails.Add(new Mail
                    (
                        message["id"].ToObject<long>(),
                        message["msg_date"].ToObject<DateTime>(),
                        message["subject"].ToObject<string>(),
                        _students.Find(s => s.Id == fromUserId),
                        message["status"].ToObject<Status>()
                    ));
                }
            }
            UpdateTextEvent?.Invoke("Список писем успешно получен!");
            LoadDataSuccessEvent?.Invoke(_mails);
        }

        internal List<Mail> GetMails()
        {
            return _mails;
        }

        internal async Task DownloadMessages(List<Mail> mailsToDownload)
        {
            foreach (var group in _groups)
            {
                JObject user_list = JObject.Parse(await SendRequestAsync($"https://kip.eljur.ru/journal-api-messages-action?method=messages.get_recipients_list&key1=school&key2=students&key3=2024%2F2025_1_{System.Web.HttpUtility.UrlEncode(group.Name)}%23%23%23%23%23{group.Key}&dep=null", _cookies));

                ResetProgressEvent?.Invoke(user_list["user_list"].Count());

                foreach (var student in user_list["user_list"])
                {
                    _students.Add(new Student(student["id"].ToObject<long>(), student["firstname"].ToObject<string>(), student["lastname"].ToObject<string>(), student["middlename"].ToObject<string>(), group));
                    UpdateProgressEvent?.Invoke(1);

                }
            }

            UpdateTextEvent?.Invoke("Получаем список сообщений...");

            int limit = 150;
            string apiUrl = $"https://kip.eljur.ru/journal-api-messages-action?method=messages.get_list&category=inbox&search=&limit={limit}&offset=0&teacher=21742&status=unread&companion=&minDate=0";
            string jsonResponse = await SendRequestAsync(apiUrl, _cookies);
            JObject jsonData = JObject.Parse(jsonResponse);
            ResetProgressEvent?.Invoke(jsonData["list"].Count());
            foreach (var message in jsonData["list"])
            {
                if (message["hasFiles"].ToObject<bool>())
                {
                    foreach (var file in message["files"])
                    {
                        string fileUrl = file["url"].ToString();
                        string fileName = file["filename"].ToString();
                        string subDirectory = null;
                        long studentId = message["from_user"].ToObject<long>();
                        var student = _students.Find(s => s.Id == studentId);
                        if (student != null)
                        {
                            subDirectory = $"{student.Group.Name}/{student.LastName.Replace(" ", "")} {student.FirtsName.Replace(" ", "")}";
                        }
                        if (message["files"].Count() > 1)
                        {
                            subDirectory = $"{subDirectory}/{message["subject"].ToObject<string>()}";
                        }
                        subDirectory = Regex.Replace(subDirectory, @"[<>:""|?*]", string.Empty);
                        DownloadFile(fileUrl, fileName, subDirectory);
                        await SendRequestAsync($"https://kip.eljur.ru/journal-api-messages-action?method=messages.note_read&idsString={message["id"]}", _cookies);
                    }
                }
                UpdateProgressEvent?.Invoke(1);
            }
            UpdateTextEvent?.Invoke("Все файлы успешно скачаны!");
        }

        internal async Task<string> SendRequestAsync(string url, CookieContainer cookies)
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

        internal void DownloadFile(string fileUrl, string fileName, string subDirectory = null)
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

                if (System.IO.File.Exists(fileName))
                {
                    return;
                }

                byte[] fileBytes = client.DownloadData(fileUrl);

                System.IO.File.WriteAllBytes(fileName, fileBytes);
            }
        }

        public static void Initialize(List<CefSharp.Cookie> cefSharpCookies)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DataRepository(cefSharpCookies);
                    }
                }
            }
        }
        
        public static DataRepository GetInstance()
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("DataRepository не был инициализирован. Вызовите Initialize перед первым использованием.");
            }
            return _instance;
        }
    }
}
