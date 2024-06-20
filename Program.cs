using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.EntityFrameworkCore;
using Student_web.Models;
using Student_web.Data;
using Student_web.ClassroomData;
using TA;
using Microsoft.AspNetCore.DataProtection;
using Student_web.Services;
using static TA.Controllers.TAReserveController;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<WordDocumentGenerator>();
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddSession(
    o=>{o.IdleTimeout = TimeSpan.FromMinutes(30);}
);
//MariaDB
//實習
var connectionString = builder.Configuration.GetConnectionString("MySqlConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => {
     options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});
//TA預約
builder.Services.AddDbContext<TADbContext>(options => {
     options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});
//classroom
builder.Services.AddDbContext<ClassroomDbContext>(options => {
      options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
}); 

builder.Services.AddDataProtection()//資料保護服務
    .PersistKeysToFileSystem(new DirectoryInfo("C:/Student_web/DataProtectionKeys/"));

builder.Services.AddScoped<ILineNotifyService, LineNotifyService>();//LineNotify
var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    //The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
