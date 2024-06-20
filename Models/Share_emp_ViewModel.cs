using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_web.Models
{
    public class Share_emp_ViewModel
    {
        public Member? member {get; set;}
        public Share_emp? share_emps {get; set;}
        public Message_post? message_Post {get;set;}
        public Thumb? thumbs {get;set;}
        public Collection? collections {get;set;}
        public string? photoPath { get; set; } 
        public string? userPhoto { get; set; }
        public string? Myphoto { get; set; }//當前使用者的photo
        public Dictionary<string, string>? MemberPhoto { get; set; }   
        public bool UserHasKeeped { get; set; }
        public bool UserHasLiked { get; set; }  
    }
}