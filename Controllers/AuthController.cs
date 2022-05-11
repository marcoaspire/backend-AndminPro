using _04_API_HospitalAPP.Models;
using _04_API_HospitalAPP.Repository;
using _04_API_HospitalAPP.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace _04_API_HospitalAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly IJWTManagerRepository _jWTManager;
        public AuthController(UserContext context, IJWTManagerRepository jWTManager)
        {
            _context = context;
            this._jWTManager = jWTManager;
        }


        [HttpPost]
        //Authenticate
        public ActionResult PostLogin(LoginViewModel u)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var passwordHash = Crypto.HashPassword(u.Password);

                    var userLog = _context.Users.SingleOrDefault(user => user.Email == u.Email);

                    if (userLog == null)
                    {
                        return Unauthorized(new { ok = false, msg = "Email or password incorrect" });
                    }
                    else
                    {
                        var validate = Crypto.VerifyHashedPassword(userLog.Password, u.Password);
                        if (!validate)
                        {
                            return Unauthorized(new { ok = false, msg = "Password or Email incorrect" });

                        }
                        //create jwt
                        var token = _jWTManager.Authenticate(userLog);
                        return Ok(new { ok = true, token });

                    }


                }
                catch (Exception)
                {
                    return Unauthorized(new { ok = false, msg = "Invalid credentials" });

                }

            }

            return BadRequest(new { msg = "Error" });
        }
        

    }
}
