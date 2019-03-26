using Microsoft.AspNetCore.Identity;

namespace HelpDesk.Models.IdentityModels
{
    public class ApplicationRole : IdentityRole
    {
        public string Description { get; set; }

    }
}
