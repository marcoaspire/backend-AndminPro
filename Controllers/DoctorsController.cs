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
    public class DoctorsController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly IJWTManagerRepository _jWTManager;

        public DoctorsController(UserContext context, IJWTManagerRepository jWTManager)
        {
            _context = context;
            this._jWTManager = jWTManager;
        }
        private Tuple<string, bool> validateJWT()
        {
            string token = (Request.Headers["x-token"]);

            Trace.WriteLine(token);
            if (token == null || token.Length==0)
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
        [HttpGet]
        public ActionResult Get()
        {

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
                    return Ok(new {
                        doctors = _context.Doctors

                            /*
                            .Select(d => new
                            {

                                Doctor = d,
                                UserEmail= d.User.Email,
                                UserName = d.User.Name
                            })
                            */
                            .Include(d => d.User)
                            .ToList(),
                        ok = true, id = t.Token });
                }
                else
                {
                    return Unauthorized(new { msg = t.Token });

                }

            }
        }

        // GET: api/Doctors/5
        [HttpGet("{id}")]
        public ActionResult GetUser(int id)
        {
            var h = _context.Doctors.SingleOrDefault(hospital => hospital.HospitalID == id);
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
        public ActionResult PostDoctor(Doctor doctor)
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
                    doctor.UserID= Int32.Parse(token.Item1);
                    _context.Doctors.Add(doctor);
                    _context.SaveChanges();

                    return Ok(new { ok = true, doctor });
                }

            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { ok = false, msg = "Unexpected error, check logs", details= ex.InnerException.Message });
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                throw;
            }

        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDoctor(int id, Doctor doctor)
        {

            var d = _context.Doctors.SingleOrDefault(d => d.DoctorID == id);
            if (d == null)
            {
                return NotFound(new { ok = false, msg = "We could not find a doctor with that ID" });
            }

            _context.Entry(d).State = EntityState.Modified;

            try
            {
                //TODO: change h values to  hospital values
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }

            return Ok(new { ok = true, doctor = d });

        }

        // DELETE: api/PaymentDetail/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            try
            {
                var doctor = await _context.Doctors.FindAsync(id);
                if (doctor == null)
                {
                    return NotFound(new { ok = false, msg = "We could not find a doctor with that ID" });
                }

                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();

                return Ok(new { ok = true, msg = "Doctor deleted" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { ok = false, msg = "Unexpected error, check logs" });
            }

        }
    }
}
