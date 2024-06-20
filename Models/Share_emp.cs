using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Student_web.Models
{
    public class Share_emp
    {   
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required(ErrorMessage = "標題未填寫")]
        public string item { get; set; }

        public string studentID { get; set; }

        public string name { get; set; }

        public string dept { get; set; }

        [Required(ErrorMessage = "公司未填寫")]
        public string company { get; set; }

        public string? location { get; set; }

        public int? salary { get; set; }

        [Required(ErrorMessage = "職位未填寫")]
        public string post { get; set; }

        [Required(ErrorMessage = "工作內容未填寫")]
        public string content { get; set; }

        public string? emp_status { get; set; }

        [Required(ErrorMessage = "求學/就業經驗分享未填寫")]
        public string share_intern { get; set; }

        public DateTime create_time { get; set; }

        public int like { get; set; }

        public int keep { get; set; }
    }
}
