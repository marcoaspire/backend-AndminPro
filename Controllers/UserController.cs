using _04_API_HospitalAPP.Models;
using _04_API_HospitalAPP.Repository;
using _04_API_HospitalAPP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace _04_API_HospitalAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly IJWTManagerRepository _jWTManager;

        public UserController(UserContext context, IJWTManagerRepository jWTManager)
        {
            _context = context;
            this._jWTManager = jWTManager;
        }


        private List<User> Paginate(List<User> users, int PageNo)
        {
            int noOfRecordPerPage = 5;
            int noOfPages = (int)Math.Ceiling(Convert.ToDouble(users.Count) / Convert.ToDouble(noOfRecordPerPage));
            int noOfRecordsToSkip = (PageNo - 1) * noOfRecordPerPage;


            users = users.Skip(noOfRecordsToSkip).Take(noOfRecordPerPage).ToList();
            return users;
        }


        [HttpGet]
        public ActionResult Get(string email)
        {
            int from = 1;
            List<User> users;
            string token = (Request.Headers["x-token"]);
            Trace.WriteLine("JWT: es ");

            Trace.WriteLine(token);
            if (token == null)
            {
                return Unauthorized(new { msg = "Did not receive a token" });//401 unautorized
            }
            else
            {
                //validate jwt
                Tokens t = _jWTManager.VerifyToken(token);
                if (t.RefreshToken == true)
                {
                    try
                    {
                        from = Int32.Parse(Request.Query["from"]);

                    }
                    catch (FormatException)
                    {
                        from = 1;
                    }
                    catch (ArgumentNullException)
                    {
                        from = 1;
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                    if (email != null)
                    {
                        var u = _context.Users.SingleOrDefault(user => user.Email == email);
                        if (u != null)
                            return Ok(u);
                        else
                        {
                            return Ok(new { id = t.Token });
                        }
                    }
                    else
                    {
                        int total = _context.Users.ToList().Count();
                        users =this.Paginate(_context.Users.ToList(), from);
                        return Ok(new { users = users, ok = true, id = t.Token, total });
                    }
                    //return Ok(_context.Users.ToList());
                    //var newToken = _jWTManager.Authenticate(userLog);
                    //return Ok(new { ok = true, id = userLog.UserID, name = userLog.Name, token = newToken.Token });
                }
                else
                {
                    return Unauthorized(new { msg = t.Token });

                }

            }
            return Unauthorized(new { msg = "Invalid token" });

            /*

            if (email != null)
            {
                var u = _context.Users.SingleOrDefault(user => user.Email == email);
                if (u != null)
                    return Ok(u);
                else
                {
                    return Ok(new { });
                }
            }
            else
                return Ok(new { users = _context.Users.ToList(), ok = true });
            */



        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public ActionResult GetUser(int id)
        {
            //return Ok(_context.Users.ToList());
            var u = _context.Users.SingleOrDefault(user => user.UserID == id);
            return Ok(new
            {
                results = u,
                msg = "user",

            });


        }

        //new user
        // POST api/<AuthController>/
        //[Route("new")]

        private bool emailRegistered(string email)
        {
            Trace.WriteLine("Checando email");
            var u = _context.Users.SingleOrDefault(u => u.Email == email);
            if (u != null)
            {
                Trace.WriteLine("Checando email error");

                return true;
            }
            
            return false;
        }

        private bool valueExists(string value)
        {
            if (value != null && value != "")
                return true;
            else
                return false;
        }


        [HttpPost]
        public ActionResult PostUser(User user)
        {
            try
            {
                if (this.emailRegistered(user.Email))
                    return BadRequest(new { ok = false, msg = "Email has already been registered" });


                    var passwordHash = Crypto.HashPassword(user.Password);
                    user.Password = passwordHash;
                    _context.Users.Add(user);
                    _context.SaveChanges();
                    var token = _jWTManager.Authenticate(user);

                return Ok(new { ok = true, user,token });
               
            }
            catch(DbUpdateException ex)
            {
                Trace.WriteLine(ex.InnerException);
                return StatusCode(500, new { ok = false, msg = "Unexpected error, check logs" });
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                throw;
            }

        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserViewModel user)
        {
            Tuple<string, bool> token = validateJWT();
            try
            {
                if (Int32.Parse(token.Item1) != id)
                {
                    return Unauthorized(new { msg = "You must not update this user" });//401 unautorized
                }
                if (!token.Item2 || Int32.Parse(token.Item1) != id)
                {
                    return Unauthorized(new { msg = token.Item1 });//401 unautorized
                }
                else
                {
                    var u = _context.Users.SingleOrDefault(u => u.UserID == id);
                    if (u == null)
                    {
                        return NotFound(new { ok = false, msg = "We could not find an user with that ID" });
                    }
                    if (valueExists(user.Email))
                    {
                        if (u.Email != user.Email)
                        {
                            if (this.emailRegistered(user.Email))
                                return BadRequest(new { ok = false, msg = "Email has already been registered" });
                            else
                                u.Email = user.Email;
                        }
                    }
                    if (valueExists(user.Name))
                    {
                        u.Name = user.Name;
                    }
                    if (valueExists(user.Role))
                    {
                        u.Role = user.Role;

                    }

                    _context.Entry(u).State = EntityState.Modified;


                    await _context.SaveChangesAsync();
                    return Ok(new { ok = true, user = u });
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }
            


            

        }

        private Tuple<string, bool> validateJWT()
        {
            string token = (Request.Headers["x-token"]);

            Trace.WriteLine(token);
            if (token == null || token.Length == 0)
            {
                return new Tuple<string, bool>("Did not receive a token", false);
            }
            else
            {
                //validate jwt
                Tokens t = _jWTManager.VerifyToken(token);
                if (t.RefreshToken == true)
                {
                    return new Tuple<string, bool>(t.Token, true);
                }
                else
                    return new Tuple<string, bool>(t.Token, false);
            }
        }

        // DELETE: api/PaymentDetail/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                Tuple<string, bool> token = validateJWT();
                if (!token.Item2)
                {
                    return Unauthorized(new { msg = token.Item1 });//401 unautorized
                }
                else
                {
                    var user = await _context.Users.FindAsync(id);
                    if (user == null)
                    {
                        return NotFound(new { ok = false, msg = "We could not find an user with that ID" });
                    }

                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();

                    return Ok(new { ok = true, msg = "User deleted" });

                }

            }
            catch (Exception)
            {
                return StatusCode(500, new { ok = false, msg = "Unexpected error, check logs" });
            }
            
        }

        
    }
}
