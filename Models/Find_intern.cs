using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Student_web.Models
{
    public class Find_intern
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required(ErrorMessage ="標題未填寫")]
        public string item { get; set; }

        [Required(ErrorMessage ="公司名稱未填寫")]
        public string company_name { get; set; }

        [Required(ErrorMessage ="公司位置未填寫")]
        public string location { get; set; }

        [Required(ErrorMessage ="應徵內容未填寫")]
        public string work_Content { get; set; }
        public int? salary { get; set; }
        public string? links { get; set; }
        public DateTime create_time { get; set; } 
        public int like { get; set; }
        public int keep { get; set; }
        public string? CompanyCode { get; set; }
    }
}