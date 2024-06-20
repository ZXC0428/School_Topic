using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Student_web.Models
{
    public class Login_History
    {
        public int id { get; set; }
        public string? IP_Address { get; set; }
        public string? studentID { get; set; }
        public DateTime login_time { get; set; }
    }
}