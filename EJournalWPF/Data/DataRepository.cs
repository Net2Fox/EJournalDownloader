using EJournalWPF.Model;
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
        private readonly CookieContainer _cookies;
        private readonly Action<string> _updateDownloadText;
        private readonly Action<int> _updateDownloadProgress;
        private readonly Action<int> _resetDownloadProgress;

        private readonly List<Group> _groups;
        private readonly List<Student> _students;

        public DataRepository(List<CefSharp.Cookie> cefSharpCookies, Action<string> updateDownloadText, Action<int> updateDownloadProgress, Action<int> resetDownloadProgress)
        {
            foreach (var cookie in cefSharpCookies)
            {
                _cookies.Add(new Uri("https://kip.eljur.ru"), new System.Net.Cookie(cookie.Name, cookie.Value));
            }

            _updateDownloadText = updateDownloadText;
            _updateDownloadProgress = updateDownloadProgress;
            _resetDownloadProgress = resetDownloadProgress;

            Task.Run(async () =>
            {
                await GetStudentsData();
            });
        }

        internal async Task GetStudentsData()
        {
            // Получаем список групп
            _updateDownloadText("Получаем список групп...");
            JObject recipient_structure = JObject.Parse(await SendRequestAsync("https://kip.eljur.ru/journal-api-messages-action?method=messages.get_recipient_structure", _cookies));
            _updateDownloadProgress(recipient_structure["structure"].Count());

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
                                _updateDownloadProgress(1);
                            }
                        }
                    }
                }
            }
            _updateDownloadText("Получаем список студентов...");

            // Получаем список студентов
            var studentTasks = _groups.Select(async group =>
            {
                JObject user_list = JObject.Parse(await SendRequestAsync($"https://kip.eljur.ru/journal-api-messages-action?method=messages.get_recipients_list&key1=school&key2=students&key3=2024%2F2025_1_{System.Web.HttpUtility.UrlEncode(group.Name)}%23%23%23%23%23{group.Key}&dep=null", _cookies));

                //_resetDownloadProgress(user_list["user_list"].Count());

                foreach (var student in user_list["user_list"])
                {
                    _students.Add(new Student(student["id"].ToObject<long>(), student["firstname"].ToObject<string>(), student["lastname"].ToObject<string>(), student["middlename"].ToObject<string>(), group));
                    //_updateDownloadProgress(1);
                }
            });

            // Ждем завершения всех заданий
            await Task.WhenAll(studentTasks);


            
        }

        internal async Task GetMessages()
        {
            foreach (var group in _groups)
            {
                JObject user_list = JObject.Parse(await SendRequestAsync($"https://kip.eljur.ru/journal-api-messages-action?method=messages.get_recipients_list&key1=school&key2=students&key3=2024%2F2025_1_{System.Web.HttpUtility.UrlEncode(group.Name)}%23%23%23%23%23{group.Key}&dep=null", _cookies));
                
                _resetDownloadProgress(user_list["user_list"].Count());
            
                foreach (var student in user_list["user_list"])
                {
                    _students.Add(new Student(student["id"].ToObject<long>(), student["firstname"].ToObject<string>(), student["lastname"].ToObject<string>(), student["middlename"].ToObject<string>(), group));
                    _updateDownloadProgress(1);
            
                }
            }

            _updateDownloadText("Получаем список сообщений...");
            
            int limit = 150;
            string apiUrl = $"https://kip.eljur.ru/journal-api-messages-action?method=messages.get_list&category=inbox&search=&limit={limit}&offset=0&teacher=21742&status=unread&companion=&minDate=0";
            string jsonResponse = await SendRequestAsync(apiUrl, _cookies);
            JObject jsonData = JObject.Parse(jsonResponse);
            _resetDownloadProgress(jsonData["list"].Count());
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
                        await SendRequestAsync($"https://kip.eljur.ru/journal-api-messages-action?method=messages.note_read&idsString={message["id"]}", cookies);
                    }
                }
                _updateDownloadProgress(1);
            }
            _updateDownloadText("Все файлы успешно скачаны!");
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

                if (File.Exists(fileName))
                {
                    return;
                }

                byte[] fileBytes = client.DownloadData(fileUrl);

                System.IO.File.WriteAllBytes(fileName, fileBytes);
            }
        }
    }
}
