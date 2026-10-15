using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ReleaseTools
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string prNumber = GetRequiredVariable("PR_NUMBER");
            string branch = GetRequiredVariable("PR_BRANCH");
            string title = GetRequiredVariable("PR_TITLE");
            string prUrl = GetRequiredVariable("PR_URL");
            string outputPath = GetRequiredVariable("GITHUB_OUTPUT");

            title = title.Replace("\r", " ").Replace("\n", " ");

            string updateType;

            if (branch.StartsWith("feature/", StringComparison.Ordinal))
            {
                updateType = "minor";
            }
            else if (branch.StartsWith("hotfix/", StringComparison.Ordinal))
            {
                updateType = "patch";
            }
            else
            {
                throw new Exception("Неподдерживаемая ветка: " + branch);
            }

            string changelog = File.ReadAllText("changelog.md");

            string markerPattern =
                @"<!-- release-pr:" +
                Regex.Escape(prNumber) +
                @" version:(\d+\.\d+\.\d+) -->";

            Match previousRelease = Regex.Match(changelog, markerPattern);

            if (previousRelease.Success)
            {
                string previousVersion = previousRelease.Groups[1].Value;

                Console.WriteLine(
                    "Для этого PR версия уже создана: " + previousVersion);

                WriteOutputs(
                    outputPath,
                    previousVersion,
                    "false",
                    "",
                    "");

                return;
            }

            string oldVersion = File.ReadAllText("version").Trim();

            if (Regex.IsMatch(oldVersion, @"\A[0-9]+\.[0-9]+\.[0-9]+\z") == false)
            {
                throw new Exception("Неверный формат версии: " + oldVersion);
            }

            string[] parts = oldVersion.Split('.');

            int major = int.Parse(parts[0]);
            int minor = int.Parse(parts[1]);
            int patch = int.Parse(parts[2]);

            if (updateType == "minor")
            {
                minor = minor + 1;
                patch = 0;
            }
            else
            {
                patch = patch + 1;
            }

            string newVersion = major + "." + minor + "." + patch;

            string heading = "# История изменений";
            string oldEntries = changelog.Trim();

            if (oldEntries.StartsWith(heading, StringComparison.Ordinal))
            {
                oldEntries = oldEntries.Substring(heading.Length).TrimStart();
            }

            string date = DateTime.UtcNow.ToString("yyyy-MM-dd");

            string entry = "## " + newVersion + " — " + date + "\n\n";
            entry = entry + "- Изменение: " + title + "\n";
            entry = entry + "- Ветка: `" + branch + "`\n";
            entry = entry + "- PR: [#" + prNumber + "](" + prUrl + ")\n\n";

            entry = entry +
                "<!-- release-pr:" + prNumber +
                " version:" + newVersion + " -->\n";

            string newChangelog =
                heading + "\n\n" +
                entry + "\n" +
                oldEntries + "\n";

            File.WriteAllText("version", newVersion + "\n");
            File.WriteAllText("changelog.md", newChangelog);

            Console.WriteLine(
                "Версия обновлена: " + oldVersion + " -> " + newVersion);

            WriteOutputs(
                outputPath,
                newVersion,
                "true",
                oldVersion,
                updateType);
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

        private static void WriteOutputs(
            string outputPath,
            string version,
            string changed,
            string oldVersion,
            string updateType)
        {
            string output = "version=" + version + "\n";
            output = output + "tag=v" + version + "\n";
            output = output + "changed=" + changed + "\n";
            output = output + "old_version=" + oldVersion + "\n";
            output = output + "update_type=" + updateType + "\n";

            File.AppendAllText(outputPath, output);
        }
    }
}