﻿using HelpDesk.DAL;
using HelpDesk.Models.Enums;
using HelpDesk.Models.IdentityEntities;
using HelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly MyContext _dbContext;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, MyContext dbContext, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser()
            {
                Email = model.Email,
                Name = model.Name,
                Surname = model.Surname,
                UserName = model.UserName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await CreateRoles();
                if (_userManager.Users.Count() == 1)
                {
                    await _userManager.AddToRoleAsync(user, IdentityRoles.Admin.ToString());
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, IdentityRoles.Client.ToString());
                }
                return RedirectToAction(nameof(Login));
            }
            else
            {
                var errMsg = "";
                foreach (var identityError in result.Errors)
                {
                    errMsg += identityError.Description;
                }
                ModelState.AddModelError(String.Empty, errMsg);
                return View(model);
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, true);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError(String.Empty, "Kullanıcı adı veya şifre hatalı");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private async Task CreateRoles()
        {
            var roleNames = Enum.GetNames(typeof(IdentityRoles));
            foreach (var roleName in roleNames)
            {
                if (!_roleManager.RoleExistsAsync(roleName).Result)
                {
                    await _roleManager.CreateAsync(new ApplicationRole()
                    {
                        Name = roleName
                    });
                }
            }

        }
    }
}