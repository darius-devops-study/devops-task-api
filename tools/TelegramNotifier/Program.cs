using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TelegramNotifier
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            string token = GetRequiredVariable("TELEGRAM_BOT_TOKEN");
            string chatId = GetRequiredVariable("TELEGRAM_CHAT_ID");
            string version = GetRequiredVariable("RELEASE_VERSION");
            string tag = GetRequiredVariable("RELEASE_TAG");
            string repository = GetRequiredVariable("GITHUB_REPOSITORY");
            string image = GetRequiredVariable("DOCKERHUB_IMAGE");
            string prUrl = GetRequiredVariable("PR_URL");
            string author = GetRequiredVariable("PR_AUTHOR");
            string changelogPath = GetRequiredVariable("CHANGELOG_PATH");

            if (File.Exists(changelogPath) == false)
            {
                Console.Error.WriteLine("Файл changelog не найден.");
                return 1;
            }

            string repositoryUrl = "https://github.com/" + repository;
            string tagUrl = repositoryUrl + "/tree/" + tag;
            string dockerUrl = "https://hub.docker.com/r/" + image + "/tags";
            string imageName = image + ":" + tag;

            string caption = "<b>Опубликована новая версия приложения</b>\n\n";

            caption = caption +
                "Проект: " + CreateLink(repositoryUrl, repository) + "\n";

            caption = caption +
                "Версия: " + Encode(version) + "\n";

            caption = caption +
                "Дата: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") +
                " UTC\n";

            caption = caption +
                "Автор PR: " + Encode(author) + "\n\n";

            caption = caption +
                CreateLink(prUrl, "Pull Request") + " | " +
                CreateLink(tagUrl, "Git-тег " + tag) + "\n";

            caption = caption +
                CreateLink(dockerUrl, "Docker Hub") + "\n";

            caption = caption +
                "Образ: <code>" + Encode(imageName) + "</code>";

            string requestUrl =
                "https://api.telegram.org/bot" + token + "/sendDocument";

            try
            {
                using (HttpClient client = new HttpClient())
                using (MultipartFormDataContent form =
                       new MultipartFormDataContent())
                {
                    client.Timeout = TimeSpan.FromSeconds(60);

                    form.Add(new StringContent(chatId), "chat_id");
                    form.Add(new StringContent(caption), "caption");
                    form.Add(new StringContent("HTML"), "parse_mode");

                    byte[] fileBytes = File.ReadAllBytes(changelogPath);

                    form.Add(
                        new ByteArrayContent(fileBytes),
                        "document",
                        "changelog.md");

                    using (HttpResponseMessage response =
                           await client.PostAsync(requestUrl, form))
                    {
                        string responseText =
                            await response.Content.ReadAsStringAsync();

                        using (JsonDocument result =
                               JsonDocument.Parse(responseText))
                        {
                            bool success =
                                result.RootElement.GetProperty("ok").GetBoolean();

                            if (response.IsSuccessStatusCode == false ||
                                success == false)
                            {
                                string? description =
                                    result.RootElement
                                        .GetProperty("description")
                                        .GetString();

                                Console.Error.WriteLine(
                                    "Telegram отклонил отправку: " + description);

                                return 1;
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException)
            {
                Console.Error.WriteLine("Не удалось подключиться к Telegram.");
                return 1;
            }
            catch (TaskCanceledException)
            {
                Console.Error.WriteLine("Истекло время ожидания Telegram.");
                return 1;
            }

            Console.WriteLine("Уведомление и changelog отправлены в Telegram.");
            return 0;
        }

        private static string GetRequiredVariable(string name)
        {
            string? value = Environment.GetEnvironmentVariable(name);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception("Не задана переменная: " + name);
            }

            return value;
        }

        private static string Encode(string value)
        {
            return WebUtility.HtmlEncode(value);
        }

        private static string CreateLink(string url, string text)
        {
            return "<a href=\"" + Encode(url) + "\">" +
                Encode(text) + "</a>";
        }
    }
}