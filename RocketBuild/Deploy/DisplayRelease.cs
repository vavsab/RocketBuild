using DevExpress.Mvvm;

namespace RocketBuild.Deploy
{
    public class DisplayRelease : BindableBase
    {
        public bool IsChecked
        {
            get => GetProperty(() => IsChecked);
            set => SetProperty(() => IsChecked, value);
        }

        public int Id { get; set; }

        public int DefinitionId { get; set; }

        public int? EnvironmentId { get; set; }

        public string Name { get; set; }

        public string AvailableVersion { get; set; }

        public string AvailableVersionLink { get; set; }

        public string LastDeployedVersion { get; set; }

        public string LastDeployedVersionLink { get; set; }

        public DisplayReleaseStatus? Status { get; set; }
    }
}