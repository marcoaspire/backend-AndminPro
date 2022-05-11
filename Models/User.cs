using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace _04_API_HospitalAPP.Models
{
    public class User 
    {

        [Key]
        public int UserID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string Img { get; set; }
        [Required]
        public string Role { get; set; } = "USER_ROLE";

        public bool Google { get; set; } =false;





    }
}
