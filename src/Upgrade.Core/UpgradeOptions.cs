namespace Upgrade
{
    public class UpgradeOptions
    {
        public string Version { get; set; } = "0.0.0";

        public string ConnectionString { get; set; }

        public string Directory { get; set; } = "./sql/";

        public int StartFromFile { get; set; }
    }
}