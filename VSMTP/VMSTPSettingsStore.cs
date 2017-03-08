using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace VSMTP
{
    internal static class VMSTPSettingsStore
    {
        public static IServiceProvider ServiceProvider;
        public const string Collection = @"InstalledProducts\VSMTP";
    }
}
