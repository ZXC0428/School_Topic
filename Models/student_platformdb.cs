using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace student_platformdb //  替換項目的命名空間  
{

    public class Classroom_information //教室資訊
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        [Key]
        public int Classinformation_id { get; set; } //主鍵
        
        [Column(TypeName = "text")]
        public string? Classroom_id { get; set; } // 教室代號

        [Column(TypeName = "text")]
        public string? Equipment { get; set; } // 設備
        [Column(TypeName = "text")]
        public string? Capacity { get; set; } // 容納人數
        [Column(TypeName = "text")]
        public string? remark { get; set; } // 容納人數
     

        [Column(TypeName = "TINYINT")]
        public byte? Classroom_state { get; set; } // 狀態
        [Column(TypeName = "text")]
        public string? Path { get; set; } // 照片位址
    }

    public class usage_rules //教室資訊
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int rules_id { get; set; } //主鍵
        
        [Column(TypeName = "text")]
        public string? rules { get; set; } // 教室代號
        
        [Column(TypeName = "DateTime")]
        public DateTime? modify_time { get; set; } //預約成功的時間

        
    }



    public class Fixed_classschedule //教師每周固定使用的教室
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Classchedule_id { get; set; } //主鍵
        [Column(TypeName = "TIME")]
        public TimeSpan? starttime_course { get; set; } // 星期
        [Column(TypeName = "TIME")]
        public TimeSpan? endtime_course { get; set; } // 星期

        [Column(TypeName = "TINYINT")]
        public int? Week { get; set; } // 星期
        [Column(TypeName = "text")]
        public string? Classroom_id { get; set; } // 教室代號
        [Column(TypeName = "text")]
        public string? Course_name { get; set; } // 課程名稱
        [Column(TypeName = "text")]
        public string? Teacher_name { get; set; } // 老師姓名

    }
    public class Appointment_record //借用教室的資料
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Record_id { get; set; } //主鍵
        [Column(TypeName = "TIMESTAMP")]
        public DateTime? StartTime { get; set; } // 開始日期
         [Column(TypeName = "TIMESTAMP")]
        public DateTime? EndTime { get; set; } // 結束日期
        [Column(TypeName = "text")]
        public string? Classroom_id { get; set; } // 教室代號

        [Column(TypeName = "text")]
    
        public string? Class { get; set; } // 班級

        [Column(TypeName = "varchar(50)")]
        public string? Student_id { get; set; } // 學號
        [Column(TypeName = "text")]
        public string? Student_name { get; set; } // 姓名
        [Column(TypeName = "text")]
        public string? Use { get; set; } // 用途 
        [Column(TypeName = "TINYINT")]
        public byte? State { get; set; } // 狀態
        [Column(TypeName = "DateTime")]
        public DateTime? ReservationTime { get; set; } //預約成功的時間
        [Column(TypeName = "text")]
        public string? Reason { get; set; } //拒絕預約理由

    }
   
    public class BookingViewModel
    {
        public List<Appointment_record>? CancelAppointments { get; set; }
        public List<Appointment_record>? AllBooked { get; set; }
        public List<usage_rules>? Usage_rules { get; set; }
        public List<borrow_key>? borrow_key { get; set; }

        public List<Classroom_information>? Edit_information { get; set; }
    }


    public class management_request
    {
        public DateTime StartTime { get; set; } 
        public DateTime EndTime { get; set; } 
        public DateTime borrow_time { get; set; } 
        public string Classroom_id { get; set; } 
        public string Student_id { get; set; } 
        public string RuleData { get; set; } 
        public string deleteClassroom { get; set; } 
        public string Reason { get; set; } 
        public string borrow { get; set; } 
    }


    public class borrow_key //借用鑰匙的資料
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int borrow_id { get; set; } //主鍵
        [Column(TypeName = "TIMESTAMP")]
        public DateTime? borrow_time { get; set; } // 借用時間
        [Column(TypeName = "text")]
        public string? Classroom_id { get; set; } // 教室代號
        [Column(TypeName = "text")]
        public string? Class { get; set; } // 班級
        [Column(TypeName = "varchar(50)")]
        public string? Student_id { get; set; } // 學號
        [Column(TypeName = "text")]
        public string? Student_name { get; set; } // 姓名
        [Column(TypeName = "DateTime")]
        public DateTime? return_time { get; set; } //歸還時間

    }
   


   
}





/*
 public class Classroom_usage //教室使用情形
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Classroon_usage_id { get; set; } //主鍵

        [Column(TypeName = "TIMESTAMP")]
        public DateTime? StartTime { get; set; } // 開始日期 
         [Column(TypeName = "TIMESTAMP")]
        public DateTime? EndTime { get; set; } // 結束日期
   
        [Column(TypeName = "text")]
        public string? Classroom_id { get; set; } // 教室代號
        [Column(TypeName = "text")]
        public string? Student_class { get; set; } // 學生班級
        [Column(TypeName = "text")]
        public string? Student_id { get; set; } // 學號
        [Column(TypeName = "text")]
        public string? Student_name { get; set; } // 學生姓名
  
        [Column(TypeName = "TINYINT")]
        public byte? State { get; set; } // 狀態
        [Column(TypeName = "DateTime")]
        public DateTime? ReservationTime { get; set; } //預約成功的時間
    }*/