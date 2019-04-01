using HelpDesk.BLL.Account;
using HelpDesk.BLL.Helpers;
using HelpDesk.BLL.Services.Senders;
using HelpDesk.DAL;
using HelpDesk.Models.Enums;
using HelpDesk.Models.IdentityEntities;
using HelpDesk.Models.Models;
using HelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
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
        private readonly MembershipTools _membershipTools;
        private readonly MyContext _dbContext;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, MyContext dbContext, RoleManager<ApplicationRole> roleManager, MembershipTools membershipTools)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _roleManager = roleManager;
            _membershipTools = membershipTools;
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

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                Name = model.Name,
                Surname = model.Surname,
                UserName = model.UserName
            };

            IdentityResult result = await _userManager.CreateAsync(user, model.Password);
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
                string errMsg = "";
                foreach (IdentityError identityError in result.Errors)
                {
                    errMsg += identityError.Description;
                }
                ModelState.AddModelError(string.Empty, errMsg);
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

            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, true);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı");
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
            string[] roleNames = Enum.GetNames(typeof(IdentityRoles));
            foreach (string roleName in roleNames)
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

        [HttpGet]
        [AllowAnonymous]
        public ActionResult RecoverPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<ActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            try
            {
                ApplicationUser user = await _membershipTools.UserManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, $"{model.Email} mail adresine kayıtlı bir üyeliğe erişilemedi");
                    return View(model);
                }

                string newPassword = StringHelpers.GetCode().Substring(0, 6);

                await _membershipTools.UserManager.RemovePasswordAsync(user);
                await _membershipTools.UserManager.AddPasswordAsync(user,
                    _membershipTools.UserManager.PasswordHasher.HashPassword(user, newPassword));

                //var token=await _membershipTools.UserManager.GeneratePasswordResetTokenAsync(user); 
                //await _membershipTools.UserManager.ResetPasswordAsync(user, token, newPassword);

                _dbContext.SaveChanges();

                if (_dbContext.SaveChanges() > 0)
                {
                    TempData["Message"] = new ErrorViewModel()
                    {
                        Text = $"Bir hata oluştu",
                        ActionName = "RecoverPassword",
                        ControllerName = "Account",
                        ErrorCode = 500
                    };
                    return RedirectToAction("Error500", "Home");
                }

                EmailService emailService = new EmailService();
                string body = $"Merhaba <b>{user.Name} {user.Surname}</b><br>Hesabınızın parolası sıfırlanmıştır<br> Yeni parolanız: <b>{newPassword}</b> <p>Yukarıdaki parolayı kullanarak sitemize giriş yapabilirsiniz.</p>";
                emailService.Send(new EmailModel() { Body = body, Subject = $"{user.UserName} Şifre Kurtarma" }, user.Email);
            }

            catch (Exception ex)
            {
                TempData["Message"] = new ErrorViewModel()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "RecoverPassword",
                    ControllerName = "Account",
                    ErrorCode = 500
                };
                return RedirectToAction("Error500", "Home");
            }
            TempData["Message"] = $"{model.Email} mail adresine yeni şifre gönderildi.";
            return RedirectToAction("Login", "Account");
        }

    }
}