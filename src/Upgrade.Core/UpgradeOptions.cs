namespace Upgrade
{
    public class UpgradeOptions
    {
        public int Version { get; set; }

        public string ConnectionString { get; set; }

        public string Directory { get; set; } = "./sql/";

        public int StartFromFile { get; set; }
    }
}