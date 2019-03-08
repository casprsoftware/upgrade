using System;

namespace Upgrade
{
    public class VersionInfo
    {
        public int Id { get; set; }

        public string Description { get; set; }

        // ReSharper disable once InconsistentNaming
        public DateTime TimeUTC { get; set; }

        public override string ToString()
        {
            return $"{Id} {TimeUTC:G}";
        }
    }
}