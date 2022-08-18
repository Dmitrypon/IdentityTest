using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Palantir.Identity.DTO;
using Palantir.Identity.Models;

namespace Palantir.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<PalantirUser> _userManager;
        private readonly SignInManager<PalantirUser> _signinManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountController> _logger;
        private readonly IdentityServerTools _tools;

        public AccountController(UserManager<PalantirUser> userManager, SignInManager<PalantirUser> signInManager, IEmailSender emailSender, ILogger<AccountController> logger, IdentityServerTools tools)
        {
            _userManager = userManager;
            _signinManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _tools = tools;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest request)
        {
            try
            {
                var existing = await _userManager.FindByEmailAsync(request.Email);
                if (existing != null)
                {
                    if (existing.IsApproved && !existing.EmailConfirmed)
                    {
                        await SendEmailConfirmationTokenAsync(existing);
                    }
                    return BadRequest(new { Email = "Пользователь с указанным адресом уже зарегистрирован" });
                }
                var user = new PalantirUser { UserName = request.Email, Email = request.Email };
                var result = await _userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    if (_userManager.Users.Count() == 1)
                    {
                        user.EmailConfirmed = true;
                        user.IsApproved = true;
                        await _userManager.AddToRoleAsync(user, "Administrator");
                        await _userManager.UpdateAsync(user);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                        var admins = await _userManager.GetUsersInRoleAsync("Administrator");
                        await SendNewUserRegistered(user, admins);
                    }
                    return Ok(new { done = true });
                }
                else
                {
                    var password = SelectByField(result.Errors, "password");
                    var email = SelectByField(result.Errors, "email");
                    var name = SelectByField(result.Errors, "name");
                    _logger.LogInformation("Return bad request", result.Errors.Select(e => e.Description));
                    return UnprocessableEntity(new { userName = name.ToArray(), email = email.ToArray(), password = password.ToArray() });
                }

            }
            catch (Exception error)
            {
                _logger.LogError(error, "Failed to register user");
                return UnprocessableEntity(new { error = "Неизвестная ошибка" });
            }
        }

        private IEnumerable<IdentityError> SelectByField(IEnumerable<IdentityError> errors, string name)
        {
            return errors.Where(e => e.Code.Contains(name, StringComparison.InvariantCultureIgnoreCase));
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user != null)
                {
                    if (!user.IsApproved)
                    {
                        return BadRequest(new { email = new[] { new { code = "EmailNotApproved", description = "Необходимо подтверждение Администратора" } } });
                    }

                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        await SendEmailConfirmationTokenAsync(user);
                        return BadRequest(new { email = new[] { new { code = "EmailNotActivated", description = "Необходимо подтверждение адреса" } } });
                    }

                    var result = await _signinManager.PasswordSignInAsync(user, request.Password, request.RememberMe, false);
                    if (result.Succeeded)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        var role = roles.FirstOrDefault();
                        var claims = await _userManager.GetClaimsAsync(user);

                        claims.Add(new Claim(JwtClaimTypes.Audience, "PalantirAPI"));
                        claims.Add(new Claim(JwtClaimTypes.Subject, user.Id));
                        claims.Add(new Claim(JwtClaimTypes.Name, user.UserName));
                        claims.Add(new Claim(JwtClaimTypes.Email, user.Email));
                        claims.Add(new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email));
                        if (!string.IsNullOrWhiteSpace(role))
                        {
                            claims.Add(new Claim(JwtClaimTypes.Role, role));
                        }
                        var token = await _tools.IssueJwtAsync((int)TimeSpan.FromHours(4).TotalSeconds, claims);
                        HttpContext.Response.Cookies.Append(".AspNetCore.Application.State", token, new CookieOptions
                        {
                            MaxAge = TimeSpan.FromHours(4),
                            HttpOnly = true,
                            SameSite = SameSiteMode.Lax
                        });

                        return Ok(new { done = true, token = token });
                    }
                }
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Failed to login user");
            }
            return Unauthorized();
        }

        [HttpPost("reset/{email}")]
        public async Task<IActionResult> ResetPassword(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    if (!user.IsApproved)
                    {
                        return BadRequest(new { email = new[] { new { code = "EmailNotApproved", description = "Необходимо подтверждение Администратора" } } });
                    }

                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        await SendEmailConfirmationTokenAsync(user);
                        return BadRequest(new { email = new[] { new { code = "EmailNotActivated", description = "Необходимо подтверждение адреса" } } });
                    }


                    var result = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await SendEmailResetPasswordTokenAsync(email, result);
                    return Ok(new { done = true });
                }
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Failed to login user");
            }
            return Unauthorized();
        }

        [HttpPost("new")]
        public async Task<IActionResult> SetNewPassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user != null)
                {
                    if (!user.IsApproved)
                    {
                        return BadRequest(new { email = new[] { new { code = "EmailNotApproved", description = "Необходимо подтверждение Администратора" } } });
                    }

                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        await SendEmailConfirmationTokenAsync(user);
                        return BadRequest(new { email = new[] { new { code = "EmailNotActivated", description = "Необходимо подтверждение адреса" } } });
                    }

                    var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);
                    if (result.Succeeded)
                    {
                        return Ok(new { done = true });
                    }
                    else
                    {
                        return BadRequest(result.Errors);
                    }
                }
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Failed to login user");
            }
            return Unauthorized();
        }

        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Ok(new
                {
                    Email = User.Identity.Name,
                    Id = User.FindFirstValue(JwtClaimTypes.Subject),
                    Projects = User.Claims.Where(c => int.TryParse(c.Type, out var _)).Select(c => new { ProjectId = c.Type, Access = c.Value }),
                    Role = User.FindFirstValue(JwtClaimTypes.Role)
                });
            }
            return Unauthorized();
        }

        [HttpPost("verify")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> VerifyUsers([FromBody] List<string> users)
        {
            if (users?.Any() ?? false)
            {
                foreach (var user in users)
                {
                    var stored = await _userManager.FindByIdAsync(user);
                    if (stored != null)
                    {
                        stored.IsApproved = true;
                        await _userManager.UpdateAsync(stored);
                        await SendEmailConfirmationTokenAsync(stored);
                    }
                }
                return Ok();
            }
            return BadRequest(new { users = "Отсутствуют пользователи для верификации" });
        }

        [HttpGet("confirm")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            try
            {
                if (userId == null || code == null)
                {
                    return BadRequest();
                }
                var user = await _userManager.FindByIdAsync(userId);
                var result = await _userManager.ConfirmEmailAsync(user, code);
                if (result.Succeeded)
                {
                    return Redirect($"{Request.Scheme}://{Request.Host}/#/login/mail-confirmed");
                }
                return BadRequest(result.Errors);

            }
            catch (Exception error)
            {
                _logger.LogError(error, $"Failed to confirm email for userId:{userId}");
                return BadRequest();
            }
        }

        [HttpPost("logout")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // delete authentication cookie
            await HttpContext.SignOutAsync();

            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            // set this so UI rendering sees an anonymous user
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            Response.Headers.Remove("Authorization");

            return Ok(new { done = true });
        }

        #region Mail senders
        private async Task SendEmailResetPasswordTokenAsync(string email, string token)
        {
            try
            {
                await _emailSender.SendEmailAsync(email, "Палантир. Сброс пароля", $"Для создания нового пароля перейдите по <a href=\"{Request.Scheme}://{Request.Host}/#/login/restore/{token}\">ссылке</a>");
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Failed to send email");
            }
        }

        private async Task SendEmailConfirmationTokenAsync(PalantirUser user)
        {
            try
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Scheme);
                await _emailSender.SendEmailAsync(user.Email, "Палантир. Подтверждение почтового адреса", $"Для подтверждения учётной записи перейдите по <a href=\"{callbackUrl}\">ссылке</a>");
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Failed to send email");
            }
        }

        private async Task SendNewUserRegistered(PalantirUser user, IList<PalantirUser> administrators)
        {
            try
            {
                foreach (var admin in administrators)
                {
                    await _emailSender.SendEmailAsync(admin.Email, "Регистрация нового пользователя", $"В системе зарегистрировался новый пользователь ({user.Email}). Необходимо верифицировать его.");
                }
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Failed to send email");
            }
        }
        #endregion
    }
}