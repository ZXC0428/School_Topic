using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TA //  替換項目的命名空間  
{
    public class TATable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 设置为自动递增
        public int Tacourse_id { get; set; } // 課程ID
        public DateTime? Date { get; set; } // 日期

        public TimeSpan? Startime { get; set; } // 開始時間

        public TimeSpan? Overtime { get; set; } // 結束時間

        public string? CourseName { get; set; } // 課程名稱
        public string? Ta_id { get; set; }    //TA學號

        public string? TAName { get; set; } // TA名稱

        public string? Classroom { get; set; } // 教室名稱

        internal static dynamic ToList()
        {
            throw new NotImplementedException();
        }
    }

    public class TAreserve
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 设置为自动递增
        public int Reserves_id { get; set; }  // 課程ID

        //導航屬性
        [ForeignKey("TATable")]
        public int Tacourse_id { get; set; } //課程預約

        public virtual TATable TATable { get; set; }

        public string? Student_id { get; set; }    //學生ID

        public DateTime? Reservestime { get; set; }    //預約時間
        

    }

    public class Feedback
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 设置为自动递增
        public int Feedback_id { get; set; }  // 課程ID
        public int Tacourse_id { get; set; }  // 課程ID
        public string? studentID { get; set; }  //學生ID
        public string? Txt { get; set; }  //回饋
        public int? answer_1 { get; set; }  // 回饋問題1答案 
        public int? answer_2 { get; set; }  // 回饋問題2答案
        public int? answer_3 { get; set; }  // 回饋問題3答案
        public int? answer_4 { get; set; }  // 回饋問題4答案
        public int? answer_5 { get; set; }  // 回饋問題5答案

        internal static dynamic ToList()
        {
            throw new NotImplementedException();
        }
    }

    
    public class TADbContext : DbContext
    {
        public TADbContext(DbContextOptions<TADbContext> options) : base(options)
        {
            
        }

        public DbSet<TATable> TATables { get; set; } //課表
        public DbSet<TAreserve> TAreserves { get; set; } // 預約內容
        public DbSet<Feedback> Feedbacks { get; set; } //回饋
    }
}
