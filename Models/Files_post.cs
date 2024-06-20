using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace Student_web.Models
{
    public class Files_post
    {
        public int Id { get; set; }
        public int post_id { get; set; }
        public string post_Type { get; set; }
        public string file_name { get; set; }
        public string file_type { get; set; }
        public long file_size { get; set; }
        public DateTime upload_time { get; set; }
    }
}