﻿using Microsoft.AspNetCore.Identity;

namespace HelpDesk.Models.IdentityModels
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }

    }
}
