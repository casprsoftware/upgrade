using System;
using System.Collections.Generic;
using System.Text;

namespace Upgrade
{
    public class DbProviderOptions
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
