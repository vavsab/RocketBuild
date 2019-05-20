using System.Collections.Generic;

namespace RocketBuild.Deploy
{
    public class DisplayEnvironment
    {
        public string Name { get; set; }

        public List<DisplayRelease> Releases { get; set; } = new List<DisplayRelease>();
    }
}
