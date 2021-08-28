using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebApplication2.Model;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly UserManager<Customer> UserManager;

        public CustomerController(IMapper mapper, UserManager<Customer> userManager)
        {
            this.mapper = mapper;
            UserManager = userManager;
        }

        [Authorize]
        [HttpGet("test")]       //api/customer/test
       public String test()
        {
            return "Hello-world";
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(CustomerRegisterModel model)
        {
            var user = mapper.Map<Customer>(model);
            var result= await UserManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return StatusCode(201);
            }
            return BadRequest(result.Errors);

        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(UserLoginModel model)
        {
            var user = await UserManager.FindByEmailAsync(model.Email);

            if(user != null && await UserManager.CheckPasswordAsync(user, model.Password))
            {
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345"));
                var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                var tokeOptions = new JwtSecurityToken(
                    issuer: "http://localhost:5000",
                    audience: "http://localhost:5000",
                    claims: new List<Claim>(),
                    expires: DateTime.Now.AddMinutes(5),
                    signingCredentials: signinCredentials
                );
                var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
                return Ok(new { Token = tokenString });
            }

            return Unauthorized("Invaild email or password");

        }
    }
}
