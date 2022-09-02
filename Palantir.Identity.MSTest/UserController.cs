using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Palantir.Identity.Controllers;
//using Umbraco.Core.Services;
using IdentityServer3.Core.Services;

namespace Palantir.Identity.MSTest
{
    public class UserController : UsersController
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult UserInfo(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var user = _userService.Get(userId);
            return View(user);
        }

    }
}
