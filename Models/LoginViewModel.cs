using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_web.Models
{
    public class LoginViewModel
    {
        public Member? member { get; set; }
        public Login_History? login_History { get; set; }
    }
}