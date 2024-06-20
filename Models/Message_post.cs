using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_web.Models
{
    public class Message_post
    {
        public int id { get; set; }
        public int post_id { get; set; }
        public string post_Type { get; set; }
        public string student_Id { get; set; }
        public string student_Name { get; set; }
        public string message { get; set; }
        public DateTime message_time { get; set; }
    }
}