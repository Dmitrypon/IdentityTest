using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Palantir.Identity.DTO;
using Palantir.Identity.Models;

namespace Palantir.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class UsersController : ControllerBase
    {
        private readonly PalantirRoleManager _roleManager;
        private readonly PalantirUserManager _userManager;

        public UsersController(PalantirRoleManager roleManager, PalantirUserManager userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PalantirUserDTO>>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            if (!users.Any())
            {
                return NoContent();
            }
            var result = new List<PalantirUserDTO>();
            foreach (var user in users)
            {
                result.Add(await BuildDto(user));
            }
            return result;
        }

        // GET: api/Users/roles
        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserRole()
        {
            var result = new List<string>();
            var roles = await _roleManager.Roles.ToListAsync();
            foreach (var role in roles)
            {
                result.Add(role.Name);
            }
            return result;
        }

        // GET: api/Users/id:someUser
        [HttpGet("id:{id}")]
        public async Task<ActionResult<PalantirUserDTO>> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NoContent();
            }

            return await BuildDto(user);
        }

        // GET: api/Users/roles/name:Administrator
        [HttpGet("roles/name:{name}")]
        public async Task<ActionResult<string>> GetUserRoleByName(string name)
        {
            var role = await _roleManager.FindByNameAsync(name);

            if (role == null)
            {
                return NoContent();
            }

            return role.Name;
        }

        [HttpGet("claims")]
        public ActionResult<List<string>> GetClaims()
        {
            var result = new List<string>();
            foreach (var name in Enum.GetNames<PalantirClaimValue>())
            {
                result.Add(name);
            }
            return Ok(result);
        }


        // PUT: api/Roles
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("id:{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] PalantirUserDTO model)
        {
            try
            {
                if (id != model.Id) return UnprocessableEntity();
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound();
                if (!string.IsNullOrWhiteSpace(model.Role))
                {
                    var role = await _roleManager.FindByNameAsync(model.Role);
                    if (role == null) return NotFound();
                    var currents = await _userManager.GetRolesAsync(user);
                    var current = currents.FirstOrDefault();
                    if (current != null)
                    {
                        await _userManager.RemoveFromRoleAsync(user, role.Name);
                    }
                    await _userManager.AddToRoleAsync(user, role.Name);
                }
                var existing = await _userManager.GetClaimsAsync(user);
                foreach (var exist in existing)
                {
                    if (!model.Claims.Any(c => c.ProjectId.Equals(exist.Type)))
                    {
                        await _userManager.RemoveClaimAsync(user, exist);
                    }
                }
                foreach (var claim in model.Claims)
                {
                    var current = existing.FirstOrDefault(c => c.Type.Equals(claim.ProjectId));
                    if (current == null)
                    {
                        await _userManager.AddClaimAsync(user, new Claim(claim.ProjectId, Enum.GetName(claim.Claim)));
                    }
                    else
                    {
                        await _userManager.ReplaceClaimAsync(user, current, new Claim(claim.ProjectId, Enum.GetName(claim.Claim)));
                    }
                }
                return Ok();
            }
            catch (Exception error)
            {
                Problem(error.Message);
            }

            return NoContent();
        }

        [HttpGet("unverified")]
        [Authorize(Roles = "Administrator")]
        public async Task<IEnumerable<PalantirUser>> GetUnverified()
        {
            var users = await _userManager.Users.Where(u => !u.IsApproved).ToListAsync();
            return users;
        }

        [HttpGet("all")]
        [Authorize(Roles = "Administrator")]
        public async Task<IEnumerable<PalantirUserDTO>> GetAllUsers()
        {
            var result = new List<PalantirUserDTO>();
            var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
            foreach (var user in users)
            {
                var dto = await BuildDto(user);
                result.Add(dto);
            }
            return result;
        }

        // DELETE: api/Roles/5
        [HttpDelete("id:{id}")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NoContent();
            }

            await _userManager.DeleteAsync(user);

            return Ok(user);
        }

        private async Task<PalantirUserDTO> BuildDto(PalantirUser user)
        {
            var claims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            if (roles?.Any() ?? false)
            {
                var role = await _roleManager.FindByNameAsync(roles.FirstOrDefault());
                return new PalantirUserDTO(user, role.Name, claims);
            }
            return new PalantirUserDTO(user, null, claims);
        }
    }
}