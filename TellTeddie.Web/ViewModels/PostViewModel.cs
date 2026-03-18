using System.ComponentModel.DataAnnotations;
using static TellTeddie.Core.Enums.TellTeddieEnums;

namespace TellTeddie.Web.ViewModels
{
    public class PostViewModel
    {
        // Text Post
        //[Required(ErrorMessage = "You can't leave a post without writing one!")]
        public string? TextBody { get; set; }
        // Audio Post
        // This is interferring with the Text Post validation, find out why!
        //[Required(ErrorMessage = "You can't leave a post without saying anything!")]
        public string? AudioPostUrl { get; set; }
        public string? Name { get; set; }
        public string? Caption { get; set; }
    }
}
