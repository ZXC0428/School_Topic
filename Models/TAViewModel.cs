namespace TA.ViewModels
{
    public class ReservedCourseViewModel
    {
        public string CourseName { get; set; }
        public string TAName { get; set; }
        public DateTime Reservestime { get; set; }
        public string? Student_id { get; set; }    //學生ID
        public int ReservationId { get; set; }
        public bool HasFeedback { get; set; }  // 新增布爾屬性來標示是否已填寫回饋
    }

    public class CoursesViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CourseName { get; set; } = null;
        public List<TATable> Courses { get; set; } = new List<TATable>();

    }

    public class FeedbackViewModel
    {
        public string CourseName { get; set; }
        public string TAName { get; set; }
        public string? Txt { get; set; }
        public int? answer_1 { get; set; }
        public int? answer_2 { get; set; }
        public int? answer_3 { get; set; }
        public int? answer_4 { get; set; }
        public int? answer_5 { get; set; }
    }

    public class ReservedCourseViewMode2
    {
        public string? CourseName { get; set; } //課程名稱
        public string? TAName { get; set; } //TA名稱
        public DateTime CourseDay { get; set; } //課程日期
        public int ReservationId { get; set; } //課程ID
        public string? TA_ID { get; set; }    //TA學號
    }

    public class CourseViewModel
    {
        public CourseDetailsModel? CourseDetails { get; set; }
        public List<StudentModel>? Students { get; set; }
    }

    public class CourseDetailsModel
    {
        public int CourseID { get; set; } // 課程ID
        public DateTime? Day { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? Overtime { get; set; }
        public string? Coursename { get; set; }
        public string? TA_ID { get; set; }
        public string? Taname { get; set; }
        public string? Classroom { get; set; }
    }

    public class StudentModel
    {
        public string? StudentId { get; set; }
        public string? StudentName { get; set; }
    }


    

}
