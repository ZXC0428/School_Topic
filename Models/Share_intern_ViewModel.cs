using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_web.Models
{
    public class Share_intern_ViewModel
    {
        public Member? member {get; set;}
        public Share_intern? share_interns {get; set;}
        public Message_post? message_Post {get;set;}
        public Thumb? thumbs {get;set;}
        public Collection? collections {get;set;}
        public List<Files_post>? Files_posts { get; set; }
        public List<IFormFile>? NewFiles { get; set; } // 新上傳的檔案
        public List<int>? FilesToDelete { get; set; } // 要刪除的檔案的 ID
        public string? Myphoto { get; set; }//當前使用者的photo
        public string? photoPath { get; set; } 
        public string? userPhoto { get; set; }
        public Dictionary<string, string>? MemberPhoto { get; set; }   
        public bool UserHasKeeped { get; set; }
        public bool UserHasLiked { get; set; }  
    }
}