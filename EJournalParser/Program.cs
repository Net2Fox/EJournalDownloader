using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;


namespace EJournalParser
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Program program = new Program();
            //await program.ProcessMessagesAsync();
            Console.WriteLine("Введите логин:");
            string login = Console.ReadLine();
            Console.WriteLine("Введите пароль:");
            string password = Console.ReadLine();
            await program.Authorize(login, password);
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

        public async Task Authorize(string login, string password)
        {
            using (var client = new HttpClient())
            {
                // Установка нужных заголовков
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Данные для авторизации
                var values = new Dictionary<string, string>
                {
                    { "username", login },
                    { "password", password }
                };

                // Отправка POST-запроса
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://kip.eljur.ru/ajaxauthorize", content);

                // Обработка ответа
                if (response.IsSuccessStatusCode)
                {
                    var cookies = response.Headers.GetValues("Set-Cookie");
                    //Console.WriteLine(cookies.ToList()[0]);
                    await ProcessMessagesAsync(cookies.ToList()[0]);
                }
                else
                {
                    Console.WriteLine("Не удалось авторизоваться.");
                }
            }
        }

        public async Task ProcessMessagesAsync(string rawCookies)
        {
            string apiUrl = "https://kip.eljur.ru/journal-api-messages-action?method=messages.get_list&category=inbox&search=&limit=100&offset=0&teacher=21742&status=unread&companion=&minDate=0";
            CookieContainer cookies = new CookieContainer();

            // Разделение куков по строкам
            string[] cookiesArray = rawCookies.Split(new[] { "; " }, StringSplitOptions.RemoveEmptyEntries);

            // Добавление куков в CookieContainer
            foreach (var cookie in cookiesArray)
            {
                var cookieParts = cookie.Split(new[] { '=' }, 2); // Разделяем только на имя и значение
                if (cookieParts.Length == 2)
                {
                    var cookieName = cookieParts[0].Trim();
                    var cookieValue = cookieParts[1].Trim();

                    // Здесь предполагаем, что домен и путь известны, их можно задать явно
                    //cookieContainer.Add(new Cookie(cookieName, cookieValue, "/", "kip.eljur.ru"));
                    cookies.Add(new Uri("https://kip.eljur.ru"), new Cookie("cookieName", "cookieValue"));
                }
            }

            cookies.Add(new Uri("https://kip.eljur.ru"), new Cookie("schdomain", "kip"));
            cookies.Add(new Uri("https://kip.eljur.ru"), new Cookie("ej_fp", "691dd8c67f89809c5d2b6cb3fdaadbf5"));
            cookies.Add(new Uri("https://kip.eljur.ru"), new Cookie("ej_fonts", "ccb94da7fe1855fb0b95f714d84e97a9c8cfd283"));
            //cookies.Add(new Uri("https://kip.eljur.ru"), new Cookie("jwt_v_2", "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJpc3MiOiJlajplbGp1cjpraXAiLCJhdWQiOiJlajpzZXNzbiIsImp0aSI6Ijk5NjgzMGUyMjFlMGI5OTZiMDUyZDE5MjdhOWI0ZTgxIiwibmJmIjoxNzI2NTg4MTM2LCJpYXQiOjE3MjY1ODgxMzYsImV4cCI6MTcyNjY4MTczNiwiZG9tYWluIjoia2lwIiwic2VnbWVudCI6ImVsanVyIiwidWlkIjoyMTc0MiwibXAiOmZhbHNlfQ.LzHnowo-SjzoqMLu6GykKv7ldmylHTClXyf_UghNRwZAP0Yr20iMPM_Mb0lHJIxRS6RxEI_w45iIuAd-Gv8cLu5H_6c3tq6-SFHtzz3VFYfjmdmiUKbhEMvhh-AD_WUYjJv0xm51x321oCKWwfrTQZ5PHpCgp-l9YvI6c32goJPblDeET_5S7iG-bGCGzK6nQ9tlWc1lI196Rdh01blLk7T1SqR1uTO5f_vMWRCeT8PQUqxlv71RyGB4NZvvEG8ol-vg221CfSAsY_rYPlAZhSvZ02nsf94L_dxCgeObORkFbfdlarwRAMg1BUxKG57EIP9Eo0gVu_L7whlwSp2tGg"));
            cookies.Add(new Uri("https://kip.eljur.ru"), new Cookie("ej_id", "57f9ff2a-ded0-4e5a-821a-093773f412fe"));
            cookies.Add(new Uri("https://kip.eljur.ru"), new Cookie("ej_check", "808baa82de9b566d96548cfc7efd6282"));
            cookies.Add(new Uri("https://kip.eljur.ru"), new Cookie("CSRF-TOKEN", "4079fda40aae3937325f9bb3fb819f1a"));
            cookies.Add(new Uri("https://kip.eljur.ru"), new Cookie("ej_id", "85cd4be1-00ee-4b9a-be39-52886d35d07e"));
            cookies.Add(new Uri("https://kip.eljur.ru"), new Cookie("ej_check", "06a5376c8571e6201e6871d8ff6268fa"));


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
