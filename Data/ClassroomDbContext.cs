using Microsoft.EntityFrameworkCore;
using Student_web.Models;
using Student_web.Data;
using student_platformdb;

namespace Student_web.ClassroomData
{
    
    public class ClassroomDbContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BookingViewModel>().HasNoKey();
       
            modelBuilder.Entity<management_request>().HasNoKey();
            // 其他配置...

            base.OnModelCreating(modelBuilder);
            modelBuilder.Ignore<BookingViewModel>();//忽略這個 不用新增到資料庫
            modelBuilder.Ignore<management_request>();//忽略這個 不用新增到資料庫
        }
        public ClassroomDbContext(DbContextOptions<ClassroomDbContext> options) : base(options)
        {
           
        }
     

        // DbSet 定義模型
        public DbSet<Appointment_record> Appointment_record { get; set; }
        public DbSet<Classroom_information> Classroom_information { get; set; } // Use a unique name for DbSet

        public DbSet<Fixed_classschedule> Fixed_classschedule { get; set; }
        //public DbSet<Classroom_usage> Classroom_usage { get; set; }
        public DbSet<BookingViewModel> BookingViewModel { get; set; }
        public DbSet<usage_rules> usage_rules { get; set; }  
        public DbSet<borrow_key> borrow_key { get; set; }

    }
    
}
