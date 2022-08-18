using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Palantir.Identity.Configuration
{
    public sealed class EmailRelay
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; } = false;
        public string Login { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
    }
}
