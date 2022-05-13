using _04_API_HospitalAPP.Models;
using _04_API_HospitalAPP.Repository;
using _04_API_HospitalAPP.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Helpers;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;

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

        /*

        private async Task<object> validateGoogleToken(string token)
        {
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(u.Token);

            Debug.WriteLine(payload.Email);
            Debug.WriteLine(payload.Picture);
            Debug.WriteLine(payload.Name);

            return { name= payload.Name,picture= payload.Picture,email= payload.Email }

        }
        */
        [HttpPost("google")]
        //Authenticate
        public async Task<ActionResult> PostGoogle(LoginGoogleViewModel u)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //verify google token
                    GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(u.Token);

                    string email = payload.Email;
                    string name = payload.Name;
                    string picture = payload.Picture;

                    User userDB = _context.Users.SingleOrDefault(user => user.Email == email);
                    User newUser;
                    if (userDB == null)
                    {
                        //create a new user
                        newUser=new User() { 
                            Email=email,
                            Img=picture,
                            Name=name,
                            Google=true,
                            Password="#####"
                        };
                        _context.Users.Add(newUser);
                        _context.SaveChanges();
                    }
                    else
                    {
                        newUser = userDB;
                        newUser.Google = true;
                        //newUser.Password = "#####";
                        _context.Entry(newUser).State = EntityState.Modified;

                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return NotFound();
                        }
                    }

                    var token = _jWTManager.Authenticate(newUser);
                    //email,name,given_name,family_name,picture,jti (jwt ID)
                    return Ok(new { ok = true, token});


                }
                catch (Exception)
                {
                    return Unauthorized(new { ok = false, msg = "Invalid credentials" });

                }
            }

            return BadRequest(new { msg = ModelState.Values.SelectMany(v => v.Errors).ToList()

        });
        }


        //revalidate token
        [HttpGet]
        [Route("renew")]
        public ActionResult RefreshToken()
        {
            Debug.WriteLine("Renew2");

            string token = (Request.Headers["x-token"]);
            if (token == null)
            {
                return Ok(new { msg = "Did not receive a token, 401 Unautorized" });//401 unautorized

            }
            else
            {
                //validate jwt
                Tokens t = _jWTManager.VerifyToken(token);
                if (t.RefreshToken == true)
                {
                    var userLog = _context.Users.SingleOrDefault(user => user.UserID.ToString() == t.Token);
                    var newToken = _jWTManager.Authenticate(userLog);
                    return Ok(new { ok = true, id = userLog.UserID, name = userLog.Name, token = newToken.Token });
                }

            }
            return Ok(new { msg = "Error2" });


        }
    }
}
