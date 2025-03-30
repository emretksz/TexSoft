using Application.Interface;
using Entities.Concrete;
using Entities.Dtos;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Providers.Entities;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;
using Core.Utilities.Results;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System;

namespace WebApp.Controllers
{
    public class AccountController : Controller
    {
        //private readonly UserManager<AppUser> _userManager;
        //private readonly SignInManager<AppUser> _signInManager;

       private readonly IAppUserServices _appUserServices;
       private readonly IUserRolServices _userRolServices;

        public AccountController(IAppUserServices appUserServices, IUserRolServices userRolServices)
        {
            _appUserServices = appUserServices;
            _userRolServices = userRolServices;
        }
        //[Authorize(Policy = "AdminMagazaDepo")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> UserIndex()
        {
            var users = await _appUserServices.GetUsersAndRoles();
            return View(users);
        }

        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = (await _userRolServices.GetAll()).Data;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AppUser user)
        {
            // default tenant koydum silinecek
            user.TenantId = 1;
            var result = await _appUserServices.Add(user);
            if (result.Success)
            {
                return RedirectToAction("UserIndex");
            }
            return View();
        }
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> Update(long id)
        {
            ViewBag.Roles = (await _userRolServices.GetAll()).Data;
            var users = await _appUserServices.GetUserAndRole(id);
            return View(users);
        }

        [HttpPost]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> Update(AppUser user)
        {
            user.TenantId = 1;
            var result = await _appUserServices.UpdateAll(user);
            if (result.Success)
            {
                return RedirectToAction("UserIndex");
            }
            return View();

        }
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var res = await _appUserServices.GetById(id);
                await _appUserServices.Delete(res.Data);
                return RedirectToAction("UserIndex");
            }
            catch (System.Exception)
            {
                return RedirectToAction("UserIndex");
            }
        }
        public IActionResult SignIn()
        {
            return View();
        }
        public string HtmlScriptRegex(string htmlText)
        {
            string textReplace = htmlText;
            string regexPattern = @"[<>£#$½\[\]\}\|\{]|javascript|script";
            if (Regex.IsMatch(textReplace, regexPattern, RegexOptions.IgnoreCase))
            {
                Regex specialCharsRegex = new Regex(regexPattern, RegexOptions.IgnoreCase);
                htmlText = specialCharsRegex.Replace(textReplace, "");

            }
            return htmlText;
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(UserSingInModel signIn)
        {
  
           signIn.Password = HtmlScriptRegex(signIn.Password);
            signIn.Password = Regex.Replace(signIn.Password, @"[\[\]/@&%$#]", "");

            var result = await _appUserServices.CheckUserAsync(signIn);
            if (result!=null)
            {
                var roleResult = await _appUserServices.GetRolesByUserIdAsync(result.Id);

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name,result.UserName),
                    new Claim(ClaimTypes.Role,roleResult)
                };


                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = /*dto.RememberMe*/ true,
                };

                await HttpContext.SignInAsync(
                      CookieAuthenticationDefaults.AuthenticationScheme,
                      new ClaimsPrincipal(claimsIdentity),
                      authProperties);

                switch (roleResult)
                {
                    case "Magaza":
                        return RedirectToAction("MagazaIndex","Magaza");
                    case "Depo":
                        return RedirectToAction("TempsIndex", "Temp");
                    case "Admin":
                        return RedirectToAction("Index1", "Home");
                    default:
                        return RedirectToAction("Login", "Account");
                }
                
            }
            ModelState.AddModelError("Kullanıcı adı veya şifre hatalı","hata");
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> SignInMobile(UserSingInModel signIn)
        {

            signIn.Password = HtmlScriptRegex(signIn.Password);
            signIn.Password = Regex.Replace(signIn.Password, @"[\[\]/@&%$#]", "");

            var result = await _appUserServices.CheckUserAsync(signIn);
            if (result != null)
            {
                var roleResult = await _appUserServices.GetRolesByUserIdAsync(result.Id);
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name,result.UserName),
                    new Claim(ClaimTypes.Role,roleResult)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("BuCokGizliBirAnahtar123456789012"));
                var credits = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken
                    (
                    issuer: "https://texsoft.emretoksoz.com",
                    claims: claims, 
                    expires: DateTime.Now.AddDays(7),
                    signingCredentials: credits, 
                    audience: "https://texsoft.emretoksoz.com"
                    
                    );

            
                var createToken = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(createToken);

            }
            return BadRequest();
        }
        //[HttpPost]
        //public async Task<IActionResult> SignIn(UserSingInModel model)
        //{
        //    if (ModelState.IsValid)
        //    {

        //        var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, true);
        //        if (result.Succeeded)
        //        {
        //            var user = await _userManager.FindByNameAsync(model.UserName);
        //            var roles = await _userManager.GetRolesAsync(user);
        //            if (roles.Contains("Admin"))
        //            {
        //                return RedirectToAction("Index","Home");
        //            }
        //            return RedirectToAction("Index", "Home");
        //        }
        //        else if (result.IsLockedOut)
        //        {

        //        }
        //        else if (result.IsNotAllowed)
        //        {

        //        }
        //    }
        //    return View(model);
        //}

        //public async Task<IActionResult> LogOut()
        //{
        //    await _signInManager.SignOutAsync();
        //    return RedirectToAction("Index");
        //}



        //    [HttpPost]
        //    public async Task<IActionResult> SignIn(AppUser dto)
        //    {
        //        var result = await _appUserServices.CheckUserAsync(dto);
        //        if (result.Success)
        //        {
        //            var roleResult = await _appUserServices.GetRolesByUserIdAsync(dto.Id);
        //            // ilgili kullanıcının rollerini çekmemiz.
        //            var claims = new List<Claim>();

        //            if (roleResult.ResponseType == Common.ResponseType.Success)
        //            {
        //                foreach (var role in roleResult.Data)
        //                {
        //                    claims.Add(new Claim(ClaimTypes.Role, role.Definition));
        //                }
        //            }

        //            claims.Add(new Claim(ClaimTypes.NameIdentifier, result.Data.Id.ToString()));

        //            var claimsIdentity = new ClaimsIdentity(
        //                claims, CookieAuthenticationDefaults.AuthenticationScheme);

        //            var authProperties = new AuthenticationProperties
        //            {
        //                IsPersistent = dto.RememberMe,
        //            };

        //            await HttpContext.SignInAsync(
        //                CookieAuthenticationDefaults.AuthenticationScheme,
        //                new ClaimsPrincipal(claimsIdentity),
        //                authProperties);

        //            return RedirectToAction("Index", "Home");
        //        }
        //        ModelState.AddModelError("Kullanıcı adı veya şifre hatalı", result.Message);
        //        return View(dto);
        //    }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(
    CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn");
        }
    }
}
