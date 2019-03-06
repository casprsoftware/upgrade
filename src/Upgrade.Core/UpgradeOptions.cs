namespace Upgrade
{
    public class UpgradeOptions
    {
        public const string DefaultDirectory = "./sql/";

        public int Version { get; set; }

        public string ConnectionString { get; set; }

        public string Directory { get; set; } = DefaultDirectory;

        public int? StartFromVersion { get; set; }

        public int? StartFromFile { get; set; }
    }
}