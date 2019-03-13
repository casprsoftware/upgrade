using System;
using Microsoft.Extensions.CommandLineUtils;

namespace Upgrade
{
    internal static class CommandArgumentExtensions
    {
        public static int? ToNullableInt32(this CommandArgument cmdArg)
        {
            int? val = null;
            if (!string.IsNullOrEmpty(cmdArg.Value))
            {
                val = int.Parse(cmdArg.Value);
            }
            return val;
        }
    }
}
