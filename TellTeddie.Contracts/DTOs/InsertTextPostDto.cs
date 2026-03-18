using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TellTeddie.Contracts.DTOs
{
    public class InsertTextPostDto
    {
        public PostDto? Post { get; set; }
        public TextPostDto? TextPost { get; set; }
    }
}
