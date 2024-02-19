using Azure;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers
{
    [AllowAnonymous]
    public class AuthController : BaseController
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration config;

        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.config = config;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user is null)
            {
                return Unauthorized();
            }

            if (await userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized();
            }

            var userRoles = await userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var role in userRoles) 
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = GetToken(authClaims);
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await userManager.FindByEmailAsync(model.Email);
            if (userExists is null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                        new ResponseModel { Status = "Error", Message = "User already exists!" });
            }

            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                        new ResponseModel { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            }

            return Ok(new ResponseModel { Status = "Success", Message = "User created successfully!" });
        }



        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"]!));

            var token = new JwtSecurityToken(
                issuer: config["JWT:Issuer"]!,
                audience: config["JWT:Audience"]!,
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
    }
}
