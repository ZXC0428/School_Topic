using Microsoft.EntityFrameworkCore;
using Student_web.Models;
using Student_web.Data;

namespace Student_web.Data
{
    
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {

        }

        public DbSet<career> Career {get;set;}
        public DbSet<Find_intern> find_intern {get;set;}
        public DbSet<Share_emp> share_emp {get;set;}
        public DbSet<Share_intern> share_intern {get;set;}
        public DbSet<Member> member { get; set; }
        public DbSet<Login_History> Login_History { get; set; }
        public DbSet<Message_post> Message_post {get;set;}
        public DbSet<Thumb> thumb {get;set;}
        public DbSet<Collection> collection { get; set; }
        public DbSet<Files_post> files_Post { get; set; }
        public DbSet<Randomcode> RandomCode { get; set; }
        
    }
}
