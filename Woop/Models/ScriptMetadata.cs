using FuseSharp;
using System.Collections.Generic;

namespace Woop.Models
{
    public class ScriptMetadata : IFuseable
    {
        public string Name { get; set; }

        public string Tags { get; set; }

        public string Description { get; set; }

        public string Icon { get; set; }

        public double Bias { get; set; }

        IEnumerable<FuseProperty> IFuseable.Properties => new[]
        {
            new FuseProperty(Name, 0.9),
            new FuseProperty(Tags, 0.6),
            new FuseProperty(Description, 0.2)
        };
    }
}
