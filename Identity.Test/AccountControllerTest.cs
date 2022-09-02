//using IdentityServer4;
//using Microsoft.AspNetCore.Identity.UI.Services;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.Logging;
//using Palantir.Identity.Controllers;
//using Palantir.Identity.Models;
//using Xunit;
//using Microsoft.AspNetCore.Mvc.Controllers;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
//using Xunit;
//using Palantir.Identity.Test.Fixture;
//using Palantir.Identity.DTO;
//using Microsoft.AspNet.Identity;
//using Microsoft.AspNetCore.Mvc.Routing;
//using Palantir.Identity.Data;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Fluent.Infrastructure.FluentModel;


//namespace AccountControllerTest;


//[TestClass]
//public class AccountControllerTest
//{
//    [TestMethod]
//    public void AccountController_Register_UserRegistered()
//    {
//        var accountController = new AccountController();
//        var registerViewModel = new RegisterViewModel
//        {
//            Email = "demo@demo",
//            Password = "Demo123#"
//        };

//        var result = accountController.Register(registerViewModel).Result;
//        Assert.IsTrue(result is RedirectToRouteResult);
//        Assert.IsTrue(_accountController.ModelState.All(kvp => kvp.Key != ""));
//    }
//}

