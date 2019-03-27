using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models.IdentityEntities
{
    public class ApplicationUser : IdentityUser
    {
        [Required,StringLength(50)]
        public string Name { get; set; }

        [Required,StringLength(60)]
        public string Surname { get; set; }

    }
}
