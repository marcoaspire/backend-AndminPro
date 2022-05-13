using _04_API_HospitalAPP.Models;
using _04_API_HospitalAPP.Repository;
using _04_API_HospitalAPP.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace _04_API_HospitalAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HospitalsController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly IJWTManagerRepository _jWTManager;

        public HospitalsController(UserContext context, IJWTManagerRepository jWTManager)
        {
            _context = context;
            this._jWTManager = jWTManager;
        }

        private Tuple<string,bool> validateJWT()
        {
            string token = (Request.Headers["x-token"]);

            Trace.WriteLine(token);
            if (token == null)
            {
                return new Tuple<string, bool> ("Did not receive a token", false);
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

        [HttpGet]
        public ActionResult Get()
        {

            Tuple<string, bool> token = validateJWT();
            if (!token.Item2)
            {
                return Unauthorized(new { msg = token.Item1 });//401 unautorized
            }
            else
            {
                //validate jwt
                return Ok(new { users = _context.Hospitals.Include(h => h.User).ToList(), ok = true, id = token.Item1 });
                

            }
        }

        // GET: api/Hospitals/5
        [HttpGet("{id}")]
        public ActionResult GetUser(int id)
        {
            var h = _context.Hospitals.SingleOrDefault(hospital => hospital.HospitalID == id);
            return Ok(new
            {
                results = h,
                msg = "hospital",

            });


        }

        //new user
        // POST api/<AuthController>/
        //[Route("new")]

        [HttpPost]
        public ActionResult PostHospital(Hospital hospital)
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
                    //validate jwt
                    Trace.WriteLine("ID usuario " + token.Item1);


                    hospital.UserID = Int32.Parse(token.Item1);
                    _context.Hospitals.Add(hospital);
                    _context.SaveChanges();

                    return Ok(new { ok = true, hospital });


                }
                

            }
            catch (DbUpdateException ex)
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
        public async Task<IActionResult> PutHospital(int id, HospitalViewModel hospital)
        {
            Tuple<string, bool> token = validateJWT();
            if (!token.Item2)
            {
                return Unauthorized(new { msg = token.Item1 });//401 unautorized
            }
            else
            {
                var h = _context.Hospitals.SingleOrDefault(ho => ho.HospitalID == id);
                if (h == null)
                {
                    return NotFound(new { ok = false, msg = "We could not find an user with that ID" });
                }

                _context.Entry(h).State = EntityState.Modified;

                try
                {
                    if (hospital.Name != null && hospital.Name != "" )
                        h.Name = hospital.Name;
                    h.UserID = Int32.Parse(token.Item1);
                    //TODO: change h values to  hospital values
                    await _context.SaveChangesAsync();
                    return Ok(new { ok = true, hospital = h, msg="Hospital updated" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }


            }
            

        }

        // DELETE: api/PaymentDetail/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHospital(int id)
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

                    var hospital = await _context.Hospitals.FindAsync(id);
                    if (hospital == null)
                    {
                        return NotFound(new { ok = false, msg = "We could not find an hospital with that ID" });
                    }
                    //hospital.
                    _context.Hospitals.Remove(hospital);
                    await _context.SaveChangesAsync();

                    return Ok(new { ok = true, msg = "Hospital deleted" });
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new { ok = false, msg = "Unexpected error, check logs" });
            }

        }
    }
}
