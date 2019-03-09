namespace Upgrade
{
    /// <summary>
    /// Upgrade Options
    /// </summary>
    public class UpgradeOptions
    {
        public const string DefaultDirectory = "./sql/";

        /// <summary>
        /// Target version to upgrade
        /// </summary>
        public int TargetVersion { get; set; }

        /// <summary>
        /// Connection string to database
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Directory path with sql scripts
        /// </summary>
        public string Directory { get; set; } = DefaultDirectory;

        /// <summary>
        /// Start from the version
        /// </summary>
        public int? StartVersion { get; set; }

        /// <summary>
        /// Start from the file
        /// </summary>
        public int? StartFile { get; set; }
    }
}