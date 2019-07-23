using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using RocketBuild.Settings;

namespace RocketBuild
{
    public partial class MainWindow
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();

            if (WindowPositionSettings.Current.TryGetValue(nameof(MainWindow), out WindowPosition windowPosition))
            {
                Top = windowPosition.Top;
                Left = windowPosition.Left;
                Height = windowPosition.Height;
                Width = windowPosition.Width;
                WindowState = windowPosition.WindowState;
            }
        }

        private void OnHyperlinkClick(object sender, RoutedEventArgs e)
        {
            var destination = ((Hyperlink)e.OriginalSource).NavigateUri;

            using (Process browser = new Process())
            {
                browser.StartInfo = new ProcessStartInfo
                {
                    FileName = destination.ToString(),
                    UseShellExecute = true,
                    ErrorDialog = true
                };

                browser.Start();
            }
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            WindowPositionSettings.Current[nameof(MainWindow)]
                = new WindowPosition
                {
                    Top = Top,
                    Left = Left,
                    Height = Height,
                    Width = Width,
                    WindowState = WindowState
                };

            WindowPositionSettings.Save();

            ViewModel.OnClosing();
        }
    }
}
