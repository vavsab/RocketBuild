using GalaSoft.MvvmLight;

namespace RocketBuild.Build
{
    public class DisplayBuild : ObservableObject
    {
        private bool isChecked;

        public bool IsChecked
        {
            get => isChecked;
            set => Set(ref isChecked, value);
        }

        public int DefinitionId { get; set; }

        public string Name { get; set; }

        public string LastBuild { get; set; }

        public DisplayBuildStatus? LastBuildStatus { get; set; }

        public BuildResult? LastBuildResult { get; set; }

        public string LastBuildLink { get; set; }

        public string LastCheckin { get; set; }
    }
}
