using _04_API_HospitalAPP.Models;
using _04_API_HospitalAPP.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _04_API_HospitalAPP.Repository
{
    public interface IJWTManagerRepository
    {
        Tokens Authenticate(User users);
        Tokens VerifyToken(string token);
    }
}
