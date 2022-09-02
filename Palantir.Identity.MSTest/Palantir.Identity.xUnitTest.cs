using Fluent.Infrastructure.FluentModel;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
namespace Palantir.Identity.MSTest
{
    public class AccountControllerTest
    {
        [Fact]
        public void TestUnsuccessfulLogin()
        {

            //Arrange
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(userStore.Object);
            var loginModel = new LoginViewModel

            {
                UserName = "a",
                Password = "b",
                RememberMe = false

            };
            var returnUrl = "/foo";
            var user = new ApplicationUser
            {
                UserName = loginModel.UserName
            };
            var identity = new ClaimsIdentity(DefaultAuthenticationTypes.ExternalBearer);   //ApplicationCookie);
            
            identity.AddClaim(new Claim(JwtClaimTypes.Audience, "PalantirAPI")); 
            identity.AddClaim(new Claim(JwtClaimTypes.Subject, user.Id));
            identity.AddClaim(new Claim(JwtClaimTypes.Name, user.UserName));
            identity.AddClaim(new Claim(JwtClaimTypes.Email, user.Email));
            identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email));


            userManager.Setup(um => um.FindAsync(loginModel.UserName, loginModel.Password)).Returns(Task.FromResult<ApplicationUser>(null));

            var controller = new AccountController(userManager.Object);
            var helper = new MvcMockHelper(controller);

            // Act
            var actionResult = controller.Login(loginModel, returnUrl).Result;

            // Assert
            Assert.IsTrue(actionResult is ViewResult);
            var errors = controller.ModelState.Values.First().Errors;
            Assert.AreEqual(1, errors.Count());
        }         
     }
}
