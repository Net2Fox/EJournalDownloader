using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;


namespace EJournalParser
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Program program = new Program();

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

        public async Task ProcessMessagesAsync(string rawCookies)
        {
            string apiUrl = "https://kip.eljur.ru/journal-api-messages-action?method=messages.get_list&category=inbox&search=&limit=100&offset=0&teacher=21742&status=unread&companion=&minDate=0";
            CookieContainer cookies = new CookieContainer();

            string[] cookiesArray = rawCookies.Split(new[] { "; " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var cookie in cookiesArray)
            {
                var cookieParts = cookie.Split(new[] { '=' }, 2);
                if (cookieParts.Length == 2)
                {
                    var cookieName = cookieParts[0].Trim();
                    var cookieValue = cookieParts[1].Trim();

                    cookies.Add(new Uri("https://kip.eljur.ru"), new Cookie("cookieName", "cookieValue"));
                }
            }

            string jsonResponse = await SendRequestAsync(apiUrl, cookies);
            JObject jsonData = JObject.Parse(jsonResponse);

            foreach (var message in jsonData["list"])
            {
                if (message["hasFiles"].ToObject<bool>())
                {
                    foreach (var file in message["files"])
                    {
                        string fileUrl = file["url"].ToString();
                        string fileName = file["filename"].ToString();
                        string subDirectory = null;
                        if (message["files"].Count() > 1)
                        {
                            subDirectory = message["subject"].ToString();
                        }
                        DownloadFile(fileUrl, fileName, subDirectory);
                    }
                }
            }
        }

        public void DownloadFile(string fileUrl, string fileName, string subDirectory = null)
        {
            using (WebClient client = new WebClient())
            {
                if(!Directory.Exists("Работа"))
                {
                    Directory.CreateDirectory("Работа");
                }

                if (subDirectory != null)
                {
                    if(!Directory.Exists($"Работа/{subDirectory}"))
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
                    Console.WriteLine($"Файл '{fileName}' уже скачан, пропускаем...");
                    return;
                }

                byte[] fileBytes = client.DownloadData(fileUrl);
                File.WriteAllBytes(fileName, fileBytes);
                Console.WriteLine($"Файл '{fileName}' успешно скачан.");
            }
        }

    }
}
