using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace _04_API_HospitalAPP.Models
{
    public class Doctor
    {
        /*
         *  name string re
         *  img string opc
         *  user fk re
         *  hospital fk re
         *

        
         **/

        [Key]
        public int DoctorID { get; set; }
        [Required]
        public string Name { get; set; }
        public string Img { get; set; }
        //[Required]
        public int UserID { get; set; }
        [Required]
        public int HospitalID { get; set; }

        //one to many (inverse)
        public User User { get; set; }

        public Hospital Hospital { get; set; }


    }
}
