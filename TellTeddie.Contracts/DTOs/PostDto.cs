using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TellTeddie.Core.DomainModels;

namespace TellTeddie.Contracts.DTOs
{
    public class PostDto
    {
        public int PostID { get; set; }
        public string? MediaType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? Name { get; set; }
        public string? Caption { get; set; }
        public TextPostDto? TextPost { get; set; }
        public AudioPostDto? AudioPost { get; set; }
    }
}
