using _04_API_HospitalAPP.Models;
using _04_API_HospitalAPP.Repository;
using _04_API_HospitalAPP.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace _04_API_HospitalAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchesController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly IJWTManagerRepository _jWTManager;

        public SearchesController(UserContext context, IJWTManagerRepository jWTManager)
        {
            _context = context;
            this._jWTManager = jWTManager;
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

        //[Route("api/collection")]
        [HttpGet("all/{search}")]
        //[HttpGet("GetByName/{name}")]
        public ActionResult GetQuery(string search)
        {
            Trace.WriteLine(search);

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
                    //Users
                    List<User> users = _context.Users.Where(item => item.Name.Contains(search)).ToList();
                    //Hospital
                    List<Hospital> hospitals = _context.Hospitals.Where(item => item.Name.Contains(search)).ToList();


                    //Doctors
                    List<Doctor> doctors = _context.Doctors.Where(item => item.Name.Contains(search)).ToList();

                    return Ok(new { ok = true, search, users, hospitals, doctors });
                }
                else
                {
                    return Unauthorized(new { msg = t.Token });

                }

            }

        }

        //[Route("all")]
        //[HttpGet("GetById/{Id}")]
        [HttpGet("collection/{table}/{search}")]
        public ActionResult seachByCollection(string table, string search)
        {
            table = table.ToLower();
            string token = (Request.Headers["x-token"]);
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
                    switch (table)
                    {
                        case "doctors":
                            //List<Doctor> doctors = _context.Doctors.Where(item => item.Name.Contains(search)).ToList();
                            return Ok(new { ok = true, Doctors = _context.Doctors
                                .Include(d => d.User)
                                .Include(d => d.Hospital)
                                .Where(item => item.Name.Contains(search))
                                .ToList() });
                        //return Ok(new { ok = true, search, doctors });

                        case "hospitals":
                            //Hospital
                            List<Hospital> hospitals = _context.Hospitals
                                .Include(d => d.User)
                                .Where(item => item.Name.Contains(search)).ToList();
                            return Ok(new { ok = true, search, hospitals });

                        case "users":
                            //Users
                            List<User> users = _context.Users.Where(item => item.Name.Contains(search)).ToList();
                            return Ok(new { ok = true, search, users });



                        default:
                            return BadRequest(new { ok = true, msg = "table must be doctors,users or hospitals" });

                    }

                }
                else
                {
                    return Unauthorized(new { msg = t.Token });

                }

            }

        }


        private bool deleteOldImg(string path)
        {
            if (path != null)
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                return true;

            }
            return false;

        }
        private async Task<string> SavePictureServer(IFormFile file, string table)
        {
            long size = file.Length;
            string[] name = file.FileName.Split('.');
            string ext = name[name.Length - 1];
            var permittedExtensions = new string[] { "png", "jpg", "jpeg", "gif" }; 

            if (!permittedExtensions.Contains(ext.ToLower()))
            {
                return "invalidextension";
            }
            //create a new name
            Guid guid = Guid.NewGuid();
            string newName = guid.ToString()+"."+ext;

            // Set the Image File Path.
            string filePath = $"./Uploads/{table}/{newName}";
            //Save image
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }
            return filePath;
        }

        [HttpPut("/api/upload/{table}/{id}")]

        public async Task<IActionResult> UploadFile(string table, string id)
        {
            int ID;
            try
            {
                ID = Int32.Parse(id);
            }
            catch (Exception)
            {

                return BadRequest();//401 unautorized
            }
            table = table.ToLower();
            string token = (Request.Headers["x-token"]);
            if (token == null)
            {
                return Unauthorized(new { msg = "Did not receive a token" });//401 unautorized
            }
            else
            {
                //validate jwt
                Tokens t = _jWTManager.VerifyToken(token);
                Trace.WriteLine(Request.Form.Files.Count);
                if (t.RefreshToken == true)
                {
                    switch (table)
                    {
                        case "doctors":
                            var doctor = _context.Doctors.Include(d => d.User)
                                .Include(d => d.Hospital).SingleOrDefault(d => d.DoctorID == ID);
                            if (doctor == null)
                            {
                                return NotFound();
                            }
                            //delete old img
                            this.deleteOldImg(doctor.Img);

                            /*
                            if (doctor.Img != null)
                            {
                                if (System.IO.File.Exists(doctor.Img))
                                {
                                    System.IO.File.Delete(doctor.Img);
                                }
                            }
                            */
                            
                            if (Request.Form.Files.Count > 0)
                            {
                                var file = Request.Form.Files[0];
                                string picture = await SavePictureServer(file, table);
                                if (picture == "invalidextension")
                                    return BadRequest(new { msg = "The extension is invalid" });
                                doctor.Img = picture;

                                await _context.SaveChangesAsync();
                            }
                            
                            return Ok(new
                            {
                                ok = true,
                                doctor,
                                msg = "file uploaded"
                            });
                        case "hospitals":
                            //Hospital
                            var hospital = _context.Hospitals
                                .Include(d => d.User)
                                .SingleOrDefault(h =>h.HospitalID == ID);
                            //delete old img
                            this.deleteOldImg(hospital.Img);

                            
                            if (Request.Form.Files.Count > 0)
                            {
                                var file = Request.Form.Files[0];
                                string picture = await SavePictureServer(file, table);
                                if (picture == "invalidextension")
                                    return BadRequest(new { msg = "The extension is invalid" });
                                hospital.Img = picture;

                                await _context.SaveChangesAsync();
                            }

                            return Ok(new { ok = true, hospital, msg = "file uploaded" });

                        case "users":
                            //Users
                            var user = _context.Users.SingleOrDefault(u => u.UserID == ID);
                            //delete old img
                            this.deleteOldImg(user.Img);
                            
                            if (Request.Form.Files.Count > 0)
                            {
                                var file = Request.Form.Files[0];
                                string picture = await SavePictureServer(file, table);
                                if (picture == "invalidextension")
                                    return BadRequest(new { msg = "The extension is invalid" });
                                user.Img = picture;

                                await _context.SaveChangesAsync();
                            }
                            return Ok(new { ok = true, user, msg="file uploaded" });



                        default:
                            return BadRequest(new { ok = true, msg = "table must be doctors,users or hospitals" });

                    }

                }
                else
                {
                    return Unauthorized(new { msg = t.Token });

                }

            }

        }


        [HttpGet("/api/upload/{table}/{id}")]
        public ActionResult SeachImage(string table, string id)
        {
            table = table.ToLower();
            string path= $"./Uploads/{table}/{id}";
            Byte[] b;
            if (System.IO.File.Exists(path))
            {
                b = System.IO.File.ReadAllBytes(path);     
            }
            else{
                b = System.IO.File.ReadAllBytes("./Uploads/no-img.jpg");
            }
            return File(b, "image/jpeg");

        }
    }
}
