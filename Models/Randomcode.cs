using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_web.Models
{
    public class Randomcode
    {
        public int Id {get;set;}
        public string CompanyCode {get;set;}
        public DateTime CreatedAt {get;set;}
        public bool CodeState {get;set;}
    }
}