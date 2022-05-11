using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace _04_API_HospitalAPP.ViewModels
{
    public class UpdateUser
    {
        [Required]
        public string Name { get; set; }
        [Required]

        public string Email { get; set; }
        //public string Password { get; set; }
        //public string Img { get; set; }
        [Required]
        public string Role { get; set; }

        //public bool Google { get; set; } = false;
    }
}
