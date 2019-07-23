using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Newtonsoft.Json;

namespace RocketBuild.Settings
{
    public class WindowPositionSettings : Dictionary<string, WindowPosition>
    {
        private const string FileName = "WindowPosition.json";

        private static readonly string DirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RocketBuild");

        private static string FullFilePath => Path.Combine(DirectoryPath, FileName);

        private static readonly Lazy<WindowPositionSettings> SettingsLazy =
            new Lazy<WindowPositionSettings>(Initialize);

        public static WindowPositionSettings Current => SettingsLazy.Value;

        public static void Save()
        {
            Directory.CreateDirectory(DirectoryPath);
            File.WriteAllText(FullFilePath, JsonConvert.SerializeObject(Current));
        }

        private static WindowPositionSettings Initialize()
        {
            try
            {
                return JsonConvert.DeserializeObject<WindowPositionSettings>(File.ReadAllText(FullFilePath));
            }
            catch (Exception)
            {
                return new WindowPositionSettings();
            }
        }
    }

    public class WindowPosition
    {
        public double Top { get; set; }

        public double Left { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public WindowState WindowState { get; set; }
    }
}