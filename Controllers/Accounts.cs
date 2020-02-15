using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.Accounts;
using Amazon.AspNetCore.Identity.Cognito;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAdvert.Web.Controllers
{
    public class Accounts : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _pool;

        public Accounts(
            SignInManager<CognitoUser> signInManager,
            UserManager<CognitoUser> userManager,
            CognitoUserPool pool)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _pool = pool;
        }

        #region Get Operations
        public async Task<IActionResult> Signup()
        {
            var model = new SignupModel();

            return View(model);
        }

        public async Task<IActionResult> Confirm(ConfirmModel model)
        {
            return View(model);
        }

        public async Task<IActionResult> Login(LoginModel model)
        {
            return View(model);
        }

        public async Task<IActionResult> Forgot(ForgotModel model)
        {
            return View(model);
        }

        public async Task<IActionResult> ForgotConfirm(ForgotConfirmModel model)
        {
            return View(model);
        }
        #endregion Get Operations

        #region Post Operations
        [HttpPost]
        [ActionName("Signup")]
        public async Task<IActionResult> Signup(SignupModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _pool.GetUser(model.Email);
                if (user.Status != null)
                {
                    ModelState.AddModelError("UserExists", "User already exists");
                    return View(model);
                }
                user.Attributes.Add(CognitoAttribute.Name.AttributeName, model.Email);

                var createdUser = await _userManager.CreateAsync(user, model.Password).ConfigureAwait(false);

                if (createdUser.Succeeded)
                {
                    return RedirectToAction("Confirm", new ConfirmModel { Email = model.Email });
                }
                else 
                {
                    foreach (var item in createdUser.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        [ActionName("Confirm")]
        public async Task<IActionResult> ConfirmPost(ConfirmModel model) 
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);
                if (user == null) 
                {
                    ModelState.AddModelError("NotFound", "User Not Found");
                    return View(model);
                }

                var result = await (_userManager as CognitoUserManager<CognitoUser>).ConfirmSignUpAsync(user, model.Code, true).ConfigureAwait(false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else 
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }

                    return View(model);
                }
            }

            return View(model);
        }        

        [HttpPost]
        [ActionName("Login")]
        public async Task<IActionResult> LoginPost(LoginModel model)
        {

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("", "Home");
                }
                else 
                {
                    ModelState.AddModelError("Error", "Login Failed");
                }
            }

            return View("Login", model);
        }

        [HttpPost]
        [ActionName("Forgot")]
        public async Task<IActionResult> ForgotPost(ForgotModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);
                if (user.Status == null) 
                {
                    ModelState.AddModelError("Not Found", "User doesnt exists");
                    return View(model);
                }
                try
                {
                    await user.ForgotPasswordAsync().ConfigureAwait(false);
                }
                catch (Exception exp)
                {
                    ModelState.AddModelError("Failed", exp.Message);
                    return View(model);
                }

                return RedirectToAction("ForgotConfirm", new ForgotConfirmModel { Email = model.Email });
            }

            return View(model);
        }

        [HttpPost]
        [ActionName("ForgotConfirm")]
        public async Task<IActionResult> ForgotConfirmPost(ForgotConfirmModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);
                if (user.Status == null)
                {
                    ModelState.AddModelError("Not Found", "User doesnt exists");
                    return View(model);
                }

                try 
                { 
                    await user.ConfirmForgotPasswordAsync(model.VerificationCode, model.NewPassword);
                }
                    catch (Exception exp)
                {
                    ModelState.AddModelError("Failed", exp.Message);
                    return View(model);
                }

            return RedirectToAction("", "Home");
            }

            return View(model);
        }
        #endregion Post Operations
    }
}
