using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace RocketBuild
{
    public class Settings
    {
        private const string FileName = "Global.json";

        private static readonly string DirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RocketBuild");

        private static string FullFilePath => Path.Combine(DirectoryPath, FileName);

        private static readonly Lazy<Settings> SettingsLazy = new Lazy<Settings>(Initialize);

        public static Settings Current => SettingsLazy.Value;

        public string AccountUrl { get; set; }

        public bool UseSsl { get; set; } = true;

        public string Project { get; set; }

        public string ApiKey { get; set; }

        public string ReleaseNameExtractVersionRegex { get; set; }

        public string BuildNameExtractVersionRegex { get; set; }

        public IList<int> LastSelectedBuildIds { get; } = new List<int>();

        public IDictionary<string, IEnumerable<int>> LastSelectedReleaseIds { get; } =
            new Dictionary<string, IEnumerable<int>>();

        public string LastSelectedEnvironment { get; set; }

        public static void Save()
        {
            Directory.CreateDirectory(DirectoryPath);
            File.WriteAllText(FullFilePath, JsonConvert.SerializeObject(Current));
        }

        private static Settings Initialize()
        {
            try
            {
                return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(FullFilePath));
            }
            catch (Exception)
            {
                return new Settings();
            }
        }
    }
}
