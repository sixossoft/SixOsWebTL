using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixOsTL.Infrastructure.Settings
{
    public class FtpSettings
    {
        public const string SectionName = "FtpServer";
        public string FtpHost { get; set; } = default!;
        public string FtpUsername { get; set; } = default!;
        public string FtpPassword { get; set; } = default!;

        // phần mở rộng, ko cần để ý
        public int Port { get; set; } = 21;
        public string BaseDirectory { get; set; } = "/";
        public string PublicBaseUrl { get; set; } = "";
        public int RetryCount { get; set; } = 3;
        public int TimeoutSeconds { get; set; } = 30;
        public bool UseSsl { get; set; } = false;
    }
}
