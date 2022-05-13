using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace _04_API_HospitalAPP.ViewModels
{
    public class LoginGoogleViewModel
    {
        [Required]
        public string Token { get; set; }
    }
}
