using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using _04_API_HospitalAPP.ViewModels;
using _04_API_HospitalAPP.Models;

namespace _04_API_HospitalAPP.Repository
{
    public class JWTManagerRepository : IJWTManagerRepository
    {
        Dictionary<string, string> UsersRecords = new Dictionary<string, string>
        {
            { "user1","password1"},
            { "user2","password2"},
            { "user3","password3"},
        };
        private readonly IConfiguration iconfiguration;
        public JWTManagerRepository(IConfiguration iconfiguration)
        {
            this.iconfiguration = iconfiguration;
        }
        public Tokens Authenticate(User user)
        {
            /*
            if (!UsersRecords.Any(x => x.Key == users.Name && x.Value == users.Password))
            {
                return null;
            }
            */
            // generate JSON Web Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(iconfiguration["JWT:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID+""),
                    new Claim(ClaimTypes.Name, user.Name)
                }),
                //Expires = DateTime.UtcNow.AddMinutes(10),
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);



            return new Tokens { Token = tokenHandler.WriteToken(token) };
        }
        public Tokens VerifyToken(String token)
        {
            //Trace.WriteLine("VerifyToken");

            // generate JSON Web Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(iconfiguration["JWT:Key"]);
           
            var parameters = new TokenValidationParameters()
            {
                IssuerSigningKey = new SymmetricSecurityKey(tokenKey),
                ValidateLifetime = true,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true
            };
            try
            {
                SecurityToken aux;
                //Debug.WriteLine("Validacion token");
                var principal = (ClaimsPrincipal)tokenHandler.ValidateToken(token, parameters,out aux);
                //Debug.WriteLine(principal);
                //header and payload in token
                //Debug.WriteLine(aux);



                var token2 = new JwtSecurityTokenHandler().ReadJwtToken(token);
                var id = token2.Claims.First(c => c.Type == "nameid").Value;
                var name = token2.Claims.First(c => c.Type == "unique_name").Value;
                Debug.WriteLine(id);
                Debug.WriteLine(name);

                return new Tokens { Token = id, RefreshToken = true };
            }
            catch (SecurityTokenExpiredException)
            {
                Debug.WriteLine("token expired");

                return new Tokens { Token = "Token Expired" };
            }
            catch (Exception w)
            {
                Debug.WriteLine("otra exeption");

                Trace.WriteLine("Excepciotn "+ w);
                

                return new Tokens { Token = "Invalid token" };
            }

        }




    }
}
