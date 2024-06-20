using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_web.Models
{
    public class MessageViewModel
    {
        public Member? member {get; set;}
        public Find_intern? find_interns {get; set;}
        public Message_post? message_Post {get;set;}
        public Thumb? thumbs {get;set;}
        public Collection? collections {get;set;}
        public List<Files_post>? Files_posts { get; set; }
        public List<IFormFile>? NewFiles { get; set; } // 新上傳的檔案
        public List<int>? FilesToDelete { get; set; } // 要刪除的檔案的 ID
        public string? userPhoto { get; set; }//貼文的photo
        public string? Myphoto { get; set; }//當前使用者的photo
        public Dictionary<string, string>? MemberPhoto { get; set; }//留言的photo
        public bool UserHasKeeped { get; set; }//是否有珍藏資料的布林值
        public bool UserHasLiked { get; set; }//是否有按讚資料的布林值  
    }
}