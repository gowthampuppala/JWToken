using JWToken.Data;
using JWToken.Model;
using JWToken.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JWToken.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserService _userService;
        private readonly UserDbContext _db;
        public UserController(IUserService userService, UserDbContext db)
        {
            _userService = userService;
            _db = db;
        }
       

        // POST api/user/login
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] Login login)
        {
            string pass = _userService.EncodePassword(login.Password);
            User user = _db.Users.Where(s => s.Email == login.Email).FirstOrDefault();
            if (user == null)
            {
                return BadRequest(new
                {
                    Message = "No User Found,Please Sign Up"
                });
            }
            else
            {
                if (pass != user.Password)
                {
                    return BadRequest(new
                    {
                        Message = "Invalid Password"
                    });
                }
                else
                {
                    var token = _userService.Login(login.Email, pass);

                    if (token == null || token == String.Empty)
                        return BadRequest(new { message = "User name or password is incorrect" });
                    //Request.Headers.Add("Authorization", "Bearer " + token);
                    Response.Cookies.Append("token", token, new Microsoft.AspNetCore.Http.CookieOptions
                    {
                        HttpOnly = true
                    });
                    //Request.Headers.Add("Authorization", "Bearer " + token);
                    return Ok("Login Successful");
                }
            }

           
        }
        [AllowAnonymous]
        [HttpPost("SignUp")]
        public IActionResult SignUp([FromBody] User user)
        {
            User newuser = _db.Users.Where(s => s.Email == user.Email).FirstOrDefault();
            if (newuser == null)
            {
                string pass = _userService.EncodePassword(user.Password);
                user.Password = pass.ToString();
                _db.Users.Add(user);
                _db.SaveChanges();
                return Ok("Successfully Registered");
            }
            else
            {
                return BadRequest(new
                {
                    Message = "User Already Exits!"
                });
            }
        }
        [Authorize]
        [HttpGet("{id}")]
        public ActionResult<User> UserData(int id)
        {
            User user = _db.Users.Where(s => s.Id == id).FirstOrDefault();
            return Ok(user);

        }

        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("token");
            return Ok();

        }

        
    }
}
