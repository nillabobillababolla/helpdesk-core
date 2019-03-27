using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models.IdentityModels
{
    public class ApplicationRole : IdentityRole
    {
        [Required,StringLength(200)]
        public string Description { get; set; }
    }
}
