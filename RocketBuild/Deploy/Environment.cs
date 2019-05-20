using System.Collections.Generic;
using GalaSoft.MvvmLight;

namespace RocketBuild.Deploy
{
    public class DisplayEnvironment : ObservableObject
    {
        public string Name { get; set; }

        public List<DisplayRelease> Releases { get; set; } = new List<DisplayRelease>();
    }
}
