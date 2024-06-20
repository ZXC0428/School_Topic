using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_web.Models
{
    public class MyTrendViewModel
    {
        public string MyPhoto {get;set;}
        public string PostPhoto {get;set;}
        public List<Member> members { get; set; }
        public List<Collection> collections { get; set; }
        public List<Thumb> thumbs { get; set; }
        public List<Find_intern> thumb_FindInterns { get; set; }
        public List<Share_emp> thumb_ShareEmps { get; set; }
        public List<Share_intern> thumb_ShareInterns { get; set; }
        public List<Find_intern> collection_FindInterns { get; set; }
        public List<Share_emp> collection_ShareEmps { get; set; }
        public List<Share_intern> collection_ShareInterns { get; set; }
        public List<Find_intern> MyFindInterns {get;set;}
        public List<Share_emp> MyShareEmps {get;set;}
        public List<Share_intern> MyShareInterns {get;set;}
        public List<Files_post> FilesPost { get; set; }
    }
}
