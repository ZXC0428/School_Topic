using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Student_web.Models
{
    public class Share_intern
    {   
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }       

        [Required(ErrorMessage ="標題未填寫")]
        public string? item { get; set; }
        public string? studentID { get; set; }

        public string? name { get; set; }

        public int year { get; set; }

        public string? dept { get; set; }

        [Required(ErrorMessage ="公司未填寫")]
        public string company_name { get; set; }

        [Required(ErrorMessage ="位置未選擇")]
        public string location {get;set;}

        [Required(ErrorMessage ="職位未填寫")]
        public string post { get; set; }

        public int? salary { get; set; }

        public string? link { get; set; }

        [Required(ErrorMessage ="工作內容未填寫")]
        public string work_content { get; set; }

        [Required(ErrorMessage ="實習經驗未填寫")]
        public string experience { get; set; }

        public DateTime create_time { get; set; }

        public int like { get; set; }

        public int keep { get; set; }

        public string? anonymous { get; set; }
    }
}