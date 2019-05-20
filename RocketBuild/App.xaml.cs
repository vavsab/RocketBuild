using System.Windows;
using Application = System.Windows.Forms.Application;

namespace RocketBuild
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            base.OnStartup(e);
        }
    }
}
