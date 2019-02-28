using System;

namespace Upgrade
{
    public class InvalidUpgradeOptionsException : Exception
    {
        public InvalidUpgradeOptionsException(string name) : base($"Invalid Option '{name}'")
        {            
        }
    }
}