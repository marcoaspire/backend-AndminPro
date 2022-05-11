using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace _04_API_HospitalAPP.Models
{
    public class Hospital
    {
        /*
         *  name string req   
         *  img string opc
         * 
         *  user req
        
         */
        [Key]
        public int HospitalID { get; set; }
        [Required]
        public string Name { get; set; }
        public string Img { get; set; }
        //[Required]
        public int UserID { get; set; }
        //one to many(inverse)
        public User User { get; set; }

    }
}
