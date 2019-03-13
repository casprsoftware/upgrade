namespace Upgrade
{
    /// <summary>
    /// Upgrade Options
    /// </summary>
    public class UpgradeOptions
    {
        public const string DefaultDirectory = "./sql/";

       
        /// <summary>
        /// Connection string to database
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Directory path with sql scripts
        /// </summary>
        public string Directory { get; set; } = DefaultDirectory;
    }
}