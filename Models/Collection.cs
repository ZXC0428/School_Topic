using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Student_web.Models
{
    public class Collection
    {
        public int id { get; set; }
        public string post_id { get; set; }
        public string post_Type { get; set; }
        public string student_Id { get; set; }
        public string student_Name { get; set; }
        public DateTime collection_click_time { get; set; }
    }
}