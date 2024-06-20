using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Student_web.Data;


namespace Student_web.Models
{
    [Keyless]
    public class career
    {
        internal readonly IEnumerable<object> Career;

        public int year { get; set; }

        public string? science { get; set; }
        public string? grad_school { get; set; }
        public string? department { get; set; }
        public string? location { get; set; }
        public string? company { get; set; }
        public string? position { get; set; }
        public int state { get; set; }
    }
}
