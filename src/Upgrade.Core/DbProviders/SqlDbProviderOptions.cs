namespace Upgrade.DbProviders
{
    /// <summary>
    /// Sql Provider Options
    /// </summary>
    public class SqlDbProviderOptions : DbProviderOptions
    {
        public SqlDbProviderOptions()
        {
            Port = 1433;
        }

        /// <summary>
        /// Connection string to database
        /// </summary>
        public string ConnectionString { get; set; }
    }
}