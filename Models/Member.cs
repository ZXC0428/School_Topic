using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Student_web.Models
{
    public class Member
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id { get; set; }
        public string? school { get; set; }
        public string? studentID { get; set; }
        public string? password { get; set; }
        public string? student_name { get; set; }
        public string? department { get; set; }
        public string? student_class { get; set; }
        public string? Level { get; set; }
        public string? myPhoto { get; set; }
        public string? AccessToken { get; set; }
        public DateTime? Updated { get; set; }
        public bool CheckNotify { get; set; }
    }

}