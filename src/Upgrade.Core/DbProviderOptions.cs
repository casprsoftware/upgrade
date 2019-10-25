namespace Upgrade
{
    /// <summary>
    /// Database provider options
    /// </summary>
    public class DbProviderOptions
    {
        /// <summary>
        /// Database host name or IP
        /// </summary>
        public string Host { get; set; } = "localhost";
        
        /// <summary>
        /// Database server port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Database name
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Database username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Database password
        /// </summary>
        public string Password { get; set; }
    }
}
