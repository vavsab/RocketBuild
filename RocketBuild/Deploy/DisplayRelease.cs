using GalaSoft.MvvmLight;

namespace RocketBuild.Deploy
{
    public class DisplayRelease : ObservableObject
    {
        private bool isChecked;

        public bool IsChecked
        {
            get => isChecked;
            set => Set(ref isChecked, value);
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