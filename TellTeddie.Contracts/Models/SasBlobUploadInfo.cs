using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TellTeddie.Contracts.Models
{
    public class SasBlobUploadInfo
    {
        public string SasUrl { get; set; } = string.Empty;
        public string BlobUrl { get; set; } = string.Empty;
        public DateTimeOffset Expiry { get; set; }
    }
}
