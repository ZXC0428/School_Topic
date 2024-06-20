using Microsoft.AspNetCore.Mvc;
using student_platformdb;// 引入 Entity Framework Core 命名空間
using System.Text.Json;
using System.Text.Json.Serialization; 
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Student_web.ClassroomData;
using Microsoft.AspNet.Identity;
using Student_web.Models;
using Newtonsoft.Json;
using Student_web.Services;
using X.PagedList.Mvc.Core;
using X.PagedList;
using X.PagedList.Mvc.Core.Fluent;
using X.PagedList.Web.Common;
using System.Linq;
using System.Threading.Tasks;
using Student_web.Data;


namespace StudentPlatform.Controllers
{
    
    
    [Student_web.Filter.LoginFilter]
    public class ClassroomReserveController : Controller
    {
        
        DateTime currentTime = DateTime.Now; //取得現在時間
  

        private readonly ClassroomDbContext _context;
        private readonly ILogger<ClassroomReserveController> _logger;
        
        private readonly ILineNotifyService _lineNotifyService;
        private readonly ApplicationDbContext _context2;

        public ClassroomReserveController(ILogger<ClassroomReserveController> logger, ClassroomDbContext context ,ApplicationDbContext context2,ILineNotifyService lineNotifyService) // 添加 MyDbContext 作為參數
        {
            _logger = logger;
            _context = context; // 初始化 MyDbContext
            _context2 = context2;
            _lineNotifyService = lineNotifyService;

        }
        
        //private readonly Student_web.ClassroomData _context; //db

        [Route("Index")]
        public IActionResult Index()
        {
           
            return View();
        }

        protected IPagedList<Appointment_record> GetPagedProcess_book(int? page, int pageSize)
        {
            // 過濾從client傳送過來有問題頁數
            if (page.HasValue && page < 1)
            return null;
            // 從資料庫取得資料
            var listUnpaged = GetDatabase_book();
            IPagedList<Appointment_record> pagelist = listUnpaged.ToPagedList(page ?? 1, pageSize);
            // 過濾從client傳送過來有問題頁數，包含判斷有問題的頁數邏輯
            if (pagelist.PageNumber != 1 && page.HasValue && page > pagelist.PageCount)
            return null;
            return pagelist;
        }
        protected IQueryable<Appointment_record> GetDatabase_book()
        {
            DateTime today = DateTime.Now.Date;
            DateTime fourteenDaysLater = today.AddDays(14).Date;
            return _context.Appointment_record
            .Where(c => c.State != 2 && c.State != 3 && c.StartTime <= fourteenDaysLater && c.StartTime >= today)
                .OrderBy(c => c.StartTime)
                .ThenBy(c => c.Classroom_id);
        }
      
        
        [Route("ClassroomReserve/classroom_booking")]
        public async Task<IActionResult> classroom_booking(int? page)
        {
            ViewBag.Message = "預約教室";
            ViewBag.nowtime = currentTime;
            DateTime today = DateTime.Now.Date;
            DateTime fourteenDaysLater = today.AddDays(14).Date;

            const int pageSize = 4;
            //處理頁數
            ViewBag.Appointment_record = GetPagedProcess_book(page, pageSize);
            //填入頁面資料
            return View(await _context.Appointment_record.Where(c => c.State != 2 && c.State != 3 && c.StartTime <= fourteenDaysLater && c.StartTime >= today)
                .OrderBy(c => c.StartTime)
                .ThenBy(c => c.Classroom_id)
                .Skip(pageSize * ((page ?? 1) - 1))
                .Take(pageSize)
                .ToListAsync());
        }



        [HttpPost]
        [Route("ClassroomReserve/classroom_booking")]
        public  async Task<IActionResult> classroom_booking(string starttime, string endtime, string classroom ,int? page)
        {
    
            ViewBag.nowtime = currentTime;
            DateTime today = DateTime.Now.Date; //今天日期
            DateTime fourteenDaysLater = today.AddDays(5).Date;//五日後日期
            const int pageSize = 4;

            if (!string.IsNullOrEmpty(starttime) && !string.IsNullOrEmpty(endtime))
            {
            // 將字串轉換成 DateTime 物件
            DateTime startDateTime = DateTime.ParseExact(starttime, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
            DateTime endDateTime = DateTime.ParseExact(endtime, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);


            // 獲取五天後的日期 一開始
            var Classroom_usageWithinFiveDays = _context.Appointment_record
                .Where(c => c.State != 2 && c.State != 3 && c.StartTime<=fourteenDaysLater && c.StartTime>=today)
                .OrderBy(c => c.StartTime)
                .ThenBy(c => c.Classroom_id)
                .ToList();

         
            //判斷是否有人借用教室
             var isClassroomReserved2 = _context.Appointment_record
            .Any(c => c.Classroom_id == classroom
                && (
                    // 新的預約開始時間在現有預約的時間範圍內
                    (startDateTime >= c.StartTime && startDateTime <= c.EndTime)
                    // 或者新的預約結束時間在現有預約的時間範圍內
                    || (endDateTime >= c.StartTime && endDateTime <= c.EndTime)
                    // 或者新的預約完全包含在現有預約的時間範圍內
                    || (startDateTime <= c.StartTime && endDateTime >= c.EndTime)
                )
                && c.State == 1);

                if (isClassroomReserved2)
                {
                    ViewBag.ReservationMessage = "此教室已被預約，請重新所選日期、節次及教室。";
                    //處理頁數
                    ViewBag.Appointment_record = GetPagedProcess_book(page, pageSize);
                    //填入頁面資料
                    return View(await _context.Appointment_record.Where(c => c.State != 2 && c.State != 3 && c.StartTime <= fourteenDaysLater && c.StartTime >= today)
                        .OrderBy(c => c.StartTime)
                        .ThenBy(c => c.Classroom_id)
                        .Skip(pageSize * ((page ?? 1) - 1))
                        .Take(pageSize)
                        .ToListAsync());
                }
                else
                {
                    // 教室未被預約，轉跳到填寫預約教室的介面
                    return RedirectToAction("reserve_classroom", new { starttime = startDateTime, endTime = endDateTime, classroomId = classroom });
                }

            }
            else{
              

                ViewBag.ReservationMessage = "請將預約篩選填寫完整。";
                ViewBag.Appointment_record = GetPagedProcess_book(page, pageSize);
                    //填入頁面資料
                    return View(await _context.Appointment_record.Where(c => c.State != 2 && c.State != 3 && c.StartTime <= fourteenDaysLater && c.StartTime >= today)
                        .OrderBy(c => c.StartTime)
                        .ThenBy(c => c.Classroom_id)
                        .Skip(pageSize * ((page ?? 1) - 1))
                        .Take(pageSize)
                        .ToListAsync());
            }

        }

        [HttpPost]
        [Route("ClassroomReserve/UpdateReservation")]
        public IActionResult UpdateReservation(string selectedDate)
        {
            try
            {
                // 將字串轉換成 DateTime 物件
                DateTime selectedDateTime = DateTime.ParseExact(selectedDate, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);

                // 截斷日期部分，以便比對
                DateTime startOfDay = selectedDateTime.Date;

                var Classroom_usageWithinDay = _context.Appointment_record
                    .Where(c => (c.State == 0 || c.State == 1 || c.State == 4) &&
                                ((c.StartTime.Value.Date == startOfDay && c.EndTime.Value.Date == startOfDay) || // StartTime 和 EndTime 都在選擇日期的情況
                                (c.StartTime.Value.Date < startOfDay && startOfDay < c.EndTime.Value.Date) || // 選擇日期在 StartTime 和 EndTime 之間的情況
                                (c.StartTime.Value.Date < startOfDay && c.EndTime.Value.Date == startOfDay) || // EndTime 就是選擇日期的情況
                                (c.StartTime.Value.Date == startOfDay && c.EndTime.Value.Date > startOfDay))) // StartTime 就是選擇日期的情況
                    .OrderBy(c => c.StartTime)
                    .ThenBy(c => c.Classroom_id)
                    .ToList();

                // 回傳更新後的資料
                // 輸出選擇的日期和查詢結果
                Console.WriteLine($"Selected Date: {startOfDay}");
                Console.WriteLine($"Result Count: {Classroom_usageWithinDay.Count}");

                return Json(Classroom_usageWithinDay);
            }
            catch (Exception ex)
            {
                // 處理例外情況
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


        [HttpPost] //取得 有開放的教室
        [Route("ClassroomReserve/GetClassroom")]
        public IActionResult GetClassroom()
        {
            try
            {
                // 直接查詢所有 Classroom_management，不需傳遞任何參數
                var management = _context.Classroom_information.Where(c => c.Classroom_state == 1).ToList();
                Console.WriteLine("management"+management.Count);
                return Json(management);
            }
            catch (Exception ex)
            {
                // 處理錯誤
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


        //根據篩選傳遞是否有預約資料
        [HttpPost]
        [Route("ClassroomReserve/GetReservations")]
        public IActionResult GetReservations([FromBody] ReservationFilter filter)
        {
            // filter 參數包含從前端發送過來的日期、節次和教室信息
            // 根據這些信息查詢預約數據庫，這裡假設你有一個名為 _context 的 DbContext

            DateTime today = DateTime.Now.Date;//取得現在日期
            try
            {
                
                return Ok();

                
            }
            catch (Exception ex)
            {
                // 返回錯誤信息到前端
                return StatusCode(500, ex.Message);
            }
             
        }
        
        public class ReservationFilter
        {
            public DateTime? Date { get; set; }
            public string? Time { get; set; }
            public string? Classroom { get; set; }
        }

        [Route("ClassroomReserve/reserve_classroom")]
        public IActionResult reserve_classroom(string starttime, string endTime, string classroomId)
        {
            ViewBag.Message = "預約教室";
            ViewBag.nowtime = currentTime;
            
            ViewBag.starttime = starttime;
            ViewBag.endTime = endTime;
            ViewBag.ClassroomId = classroomId;
            //取得隱藏的資料!!!!!!!要加才抓的到啦
            ViewBag.Reservationstarttime = starttime;
            ViewBag.ReservationendTime = endTime;
            ViewBag.ReservationClassroomId = classroomId;

            return View();
        }

        [Route("ClassroomReserve/reserve_classroom")]
        [HttpPost]
        public IActionResult reserve_classroom(Appointment_record viewModel, string starttime, string endTime, string classroomId)
        {
            ViewBag.Message = "預約教室";
            ViewBag.nowtime = currentTime;
            //抓到登入者訊息
            var user = HttpContext.Session.GetString("User");
            var data = JsonConvert.DeserializeObject<Member>(user);
            var MemberId = data.id;//設定變數

            var MemberID = _context2.member.Find(MemberId);//去撈出id對應的member的資料
            var admins = _context2.member.Where(m => m.Level == "管理員").ToList();
            Console.WriteLine("admins"+admins);
           
            try
            {
                ViewBag.Message = "預約教室";
                ViewBag.starttime = starttime;
                ViewBag.endTime = endTime;
                ViewBag.ClassroomId = classroomId;

                if (!string.IsNullOrEmpty(starttime) && !string.IsNullOrEmpty(endTime))
                {
                    // Parse strings to DateTime
                    if (DateTime.TryParse(starttime, out DateTime startDateTime) && DateTime.TryParse(endTime, out DateTime endDateTime))
                    {
                        // Add new records to the database
                        var newAppointment_record = new Appointment_record
                        {
                            StartTime = startDateTime,
                            EndTime = endDateTime,
                            Student_id = data.studentID,
                            Class = data.student_class, //使用者班級
                            Student_name = data.student_name,
                            Use = viewModel.Use,
                            Classroom_id = classroomId, //教室
                            State = 0,  // 表已預約(待審核)
                            ReservationTime = DateTime.Now
                        };


                        _context.Add(newAppointment_record);
             
                        int savedChanges = _context.SaveChanges();

                        ViewBag.ReservationSuccess = true;
                        foreach(var admin in admins)
                        {
                            // 現在你擁有了每位管理者的 member ID（adminId），你可以進行相應的操作
                            _lineNotifyService.SendNotify(admin.AccessToken, $"{data.student_class} {data.studentID}已送出《{classroomId}》教室預約申請",11537,52002770);
                        }
                        

                    }
                    else
                    {
                        Console.WriteLine("Error: Failed to parse starttime or endTime.");
                        ViewBag.ReservationSuccess = false;
                    }
                }
                else
                {
                    Console.WriteLine("Error: starttime or endTime is null or empty.");
                    ViewBag.ReservationSuccess = false;
                }
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                ViewBag.ReservationSuccess = false;
                ViewBag.ErrorMessage = "An error occurred: " + ex.Message;
            }

            return View();
        }


        

        

        [Route("ClassroomReserve/classroom_information")]
        public IActionResult classroom_information()
        {
            ViewBag.Message = "教室各項資訊";
            var Classroom_information = _context.Classroom_information
            .OrderBy(r => r.Classroom_id)
                .ToList();
            return View(Classroom_information);

        }


        [Route("ClassroomReserve/usage_rules")]
        public IActionResult usage_rules()
        {
            ViewBag.nowtime = currentTime;
            ViewBag.Date = currentTime.ToString("yyyy-MM-dd");
            //var maxId = _context.usage_rules.Max(ur => ur.rules_id);
            var usage_rules = _context.usage_rules
                .OrderByDescending(r => r.modify_time) // 按照時間列進行遞減排序
                .ToList(); // 轉換為列表

            
            if (usage_rules.Any())
            {
                var latestUsageRule = usage_rules.First(); 
                return View(new List<usage_rules> { latestUsageRule }); // 將單個對象放入列表中傳遞給視圖
            }
            else
            {
                // 如果列表為空，傳遞一個空的模型到前端
                 return View(new List<usage_rules>());
            }

            //return View(latestUsageRule);
        }

///-----------------------------------------------------------------------------------------------------------------------------------------------------
        protected IPagedList<Appointment_record> GetPagedProcess_alreadybook(int? page, int pageSize)
        {
            // 過濾從client傳送過來有問題頁數
            if (page.HasValue && page < 1)
            return null;
            // 從資料庫取得資料
            var listUnpaged = GetDatabase_alreadybook();
            IPagedList<Appointment_record> pagelist = listUnpaged.ToPagedList(page ?? 1, pageSize);
            // 過濾從client傳送過來有問題頁數，包含判斷有問題的頁數邏輯
            if (pagelist.PageNumber != 1 && page.HasValue && page > pagelist.PageCount)
            return null;
            return pagelist;
        }
        protected IQueryable<Appointment_record> GetDatabase_alreadybook()
        {
            DateTime today = DateTime.Now.Date;
            DateTime fourteenDaysLater = today.AddDays(14).Date;
            //抓到登入者訊息
            var user = HttpContext.Session.GetString("User");
            var data = JsonConvert.DeserializeObject<Member>(user);

            return _context.Appointment_record
            .Where(c => c.State != 2 && c.State != 3 && c.StartTime>=today && c.Class==data.student_class
                && c.Student_name==data.student_name && c.Student_id==data.studentID)
                .OrderBy(c => c.StartTime)
                .ThenBy(c => c.Classroom_id);
        }
      
        
        [Route("ClassroomReserve/already_booked")]
        public async Task<IActionResult> already_booked(int? page)
        {
            ViewBag.nowtime = currentTime;
            DateTime today = DateTime.Now.Date;
            DateTime fourteenDaysLater = today.AddDays(14).Date;
            //抓到登入者訊息
            var user = HttpContext.Session.GetString("User");
            var data = JsonConvert.DeserializeObject<Member>(user);

            const int pageSize = 6;
            //處理頁數
            ViewBag.Appointment_record = GetPagedProcess_alreadybook(page, pageSize);
            //填入頁面資料
            return View(await _context.Appointment_record.Where(c => c.State != 2 && c.State != 3 && c.StartTime>=today && c.Class==data.student_class
                && c.Student_name==data.student_name && c.Student_id==data.studentID)
                .OrderBy(c => c.StartTime)
                .ThenBy(c => c.Classroom_id)
                .Skip(pageSize * ((page ?? 1) - 1))
                .Take(pageSize)
                .ToListAsync());
        }
        [HttpPost]
        [Route("ClassroomReserve/already_booked")]
        public async Task<IActionResult> already_booked()
        {
            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();

                    // 解析 body 中的 JSON 數據 StartTime EndTime
                    var requestData = System.Text.Json.JsonSerializer.Deserialize<Appointment_record>(body, new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    });
                    Console.WriteLine("Received JSON data: " + body);
                    
                    Console.WriteLine("requestData.StartTime: " + requestData.StartTime);
                    var user = HttpContext.Session.GetString("User");
                    var data = JsonConvert.DeserializeObject<Member>(user);
                    var MemberId = data.id;//設定變數
                    var MemberID = _context2.member.Find(MemberId);//去撈出id對應的member的資料
       

                    if (requestData.StartTime.HasValue && requestData.EndTime.HasValue)
                    {
                        
                            // 使用格式化后的字符串与数据库中的 DateTime 字段进行比较
                            var reservationToDelete1 = _context.Appointment_record
                            .AsEnumerable()
                                .Where(record => record.StartTime != null && record.EndTime != null &&
                                                record.StartTime == requestData.StartTime &&
                                                record.EndTime == requestData.EndTime && record.Classroom_id == requestData.Classroom_id
                                                 && record.Student_id==data.studentID)
                                .ToList();

                            if (reservationToDelete1 != null)
                            {
                                foreach (var reservationTo1 in reservationToDelete1)
                                {
                                    reservationTo1.State = 3; // 表 已預約(取消)
                                }
                               
                        
                                // Save changes to the database
                                await _context.SaveChangesAsync();
                                var admins = _context2.member.Where(m => m.Level == "管理員").ToList();
                                foreach(var admin in admins)
                                {
                                    // 現在你擁有了每位管理者的 member ID（adminId），你可以進行相應的操作
                                    _lineNotifyService.SendNotify(admin.AccessToken, $"{data.student_class} {data.studentID}已取消《{requestData.Classroom_id}》教室預約申請",11537,52002738);
                                }

                                // 返回一個成功的消息
                                _lineNotifyService.SendNotify(MemberID.AccessToken, $"您已取消《{requestData.Classroom_id}》教室預約申請",11537,52002749);
                                return Json(new { success = true, message = "預約已取消" });
                            }
                            else
                            {
                                Console.WriteLine("Delete error: Reservation not found");
                                // 如果沒有找到相應的預約，返回一個失敗的消息
                                return Json(new { success = false, message = "找不到要取消的預約" });
                            }
                                
                          
                    }
                    return Json(new { success = false, message = "沒傳回來" });
                  

                    
                   
                }
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine("Save changes failed: " + ex.Message);
                return Json(new { success = false, message = "錯誤 :" + ex.Message });
            }

        }

        protected IPagedList<Appointment_record> GetPagedProcess_historybook(int? page, int pageSize)
        {
            // 過濾從client傳送過來有問題頁數
            if (page.HasValue && page < 1)
            return null;
            // 從資料庫取得資料
            var listUnpaged = GetDatabase_historybook();
            IPagedList<Appointment_record> pagelist = listUnpaged.ToPagedList(page ?? 1, pageSize);
            // 過濾從client傳送過來有問題頁數，包含判斷有問題的頁數邏輯
            if (pagelist.PageNumber != 1 && page.HasValue && page > pagelist.PageCount)
            return null;
            return pagelist;
        }
        protected IQueryable<Appointment_record> GetDatabase_historybook()
        {
            DateTime today = DateTime.Now.Date;
            DateTime fourteenDaysLater = today.AddDays(14).Date;
            //抓到登入者訊息
            var user = HttpContext.Session.GetString("User");
            var data = JsonConvert.DeserializeObject<Member>(user);

            return _context.Appointment_record
            .Where(c =>  c.Class == data.student_class
                && c.Student_name==data.student_name && c.Student_id==data.studentID)
                .OrderBy(c => c.StartTime)
                .ThenBy(c => c.Classroom_id);
        }
      
        
        [Route("ClassroomReserve/history_booked")]
        public async Task<IActionResult> history_booked(int? page)
        {
            ViewBag.nowtime = currentTime;
            DateTime today = DateTime.Now.Date;
            DateTime fourteenDaysLater = today.AddDays(14).Date;
            //抓到登入者訊息
            var user = HttpContext.Session.GetString("User");
            var data = JsonConvert.DeserializeObject<Member>(user);
            const int pageSize = 6;
            //處理頁數
            ViewBag.Appointment_record = GetPagedProcess_historybook(page, pageSize);
            //填入頁面資料
            return View(await _context.Appointment_record.Where(c =>  c.Class == data.student_class
                && c.Student_name==data.student_name && c.Student_id==data.studentID)
                .OrderBy(c => c.StartTime)
                .ThenBy(c => c.Classroom_id)
                .Skip(pageSize * ((page ?? 1) - 1))
                .Take(pageSize)
                .ToListAsync());
        }


        protected IPagedList<borrow_key> GetPagedProcess_keyhistory(int? page, int pageSize)
        {
            // 過濾從client傳送過來有問題頁數
            if (page.HasValue && page < 1)
            return null;
            // 從資料庫取得資料
            var listUnpaged = GetDatabase_keyhistory();
            IPagedList<borrow_key> pagelist = listUnpaged.ToPagedList(page ?? 1, pageSize);
            // 過濾從client傳送過來有問題頁數，包含判斷有問題的頁數邏輯
            if (pagelist.PageNumber != 1 && page.HasValue && page > pagelist.PageCount)
            return null;
            return pagelist;
        }
        protected IQueryable<borrow_key> GetDatabase_keyhistory()
        {
            //抓到登入者訊息
            var user = HttpContext.Session.GetString("User");
            var data = JsonConvert.DeserializeObject<Member>(user);

            return _context.borrow_key.Where(c =>  c.Class == data.student_class
                && c.Student_name==data.student_name && c.Student_id==data.studentID)
                .OrderByDescending(c => c.return_time == null) // 將 return_time 是 null 的記錄排到前面
                .ThenBy(c => c.borrow_time)
                .ThenBy(c => c.Classroom_id);
        }
      
        
        [Route("ClassroomReserve/keyhistory")]
        public async Task<IActionResult> keyhistory(int? page)
        {
            ViewBag.nowtime = currentTime;
            DateTime today = DateTime.Now.Date;
            //抓到登入者訊息
            var user = HttpContext.Session.GetString("User");
            var data = JsonConvert.DeserializeObject<Member>(user);
            const int pageSize = 6;
            //處理頁數
            ViewBag.borrow_key = GetPagedProcess_keyhistory(page, pageSize);
            //填入頁面資料
            return View(await _context.borrow_key.Where(c =>  c.Class == data.student_class
                && c.Student_name==data.student_name && c.Student_id==data.studentID)
                .OrderByDescending(c => c.return_time == null) // 將 return_time 是 null 的記錄排到前面
                .ThenBy(c => c.borrow_time)
                .ThenBy(c => c.Classroom_id)
                .Skip(pageSize * ((page ?? 1) - 1))
                .Take(pageSize)
                .ToListAsync());
        }


        
        [Route("ClassroomReserve/ReviseData")]
        public IActionResult ReviseData()
        {
            ViewBag.nowtime = currentTime;
            ViewBag.Date = currentTime.ToString("yyyy-MM-dd");

            return View();
        }



        //改改改
        [HttpPost]
        [Route("ClassroomReserve/ReviseData")]
        public IActionResult ReviseData(IFormCollection formData)
        {
            //ViewBag.ReservationSuccess = true; // 表示修改预约成功
            ViewBag.nowtime = DateTime.Now; // 设置当前时间
            // 修改後數據
            var startTime = formData["startTime"];
            var endTime = formData["endtime"];
        
            Console.WriteLine(startTime);

            if (formData != null && formData.ContainsKey("startTime") && !string.IsNullOrEmpty(formData["startTime"])
                                && formData.ContainsKey("endtime") && !string.IsNullOrEmpty(formData["endtime"]))
            {
                var starttime = DateTime.ParseExact(startTime, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
                var endtime = DateTime.ParseExact(endTime, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
         
                var classroom = formData["classroom"];
                var studentid = formData["student_id"];
                // 原有資料
                var startime_B = formData["startTime_B"];
                var endtime_B = formData["endTime_B"];
                var reservationtime_B = formData["reservationTime_B"];//預約資料
   

                DateTime Reservationtime_B;
                DateTime Startime_B;
                DateTime Endtime_B;

                var classroomid_B = formData["classroomId_B"].ToString();
       
                var studentid_B = formData["studentid_B"].ToString();
                var relatedClass = _context.Appointment_record.FirstOrDefault(c => c.Student_id == studentid_B);
                var student_class = relatedClass.Class;

                // 改成用預約當下的時間進行判斷
                if (!string.IsNullOrWhiteSpace(reservationtime_B)&&!string.IsNullOrWhiteSpace(startime_B)&&!string.IsNullOrWhiteSpace(endtime_B))
                {
                    Console.WriteLine(reservationtime_B);
                    // 使用正確的格式字串解析輸入的時間字符串
                    if (DateTime.TryParseExact(startime_B, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out Startime_B)
                    && DateTime.TryParseExact(endtime_B, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out Endtime_B)
                   )
                    {

                        string formattedstarTime_B = Startime_B.ToString("yyyy-MM-dd HH:mm:ss");
                        DateTime formattedStartTime = DateTime.ParseExact(formattedstarTime_B, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        string formatteReservationtime_B = reservationtime_B.ToString();
                        string formattedReservationtime_B = formatteReservationtime_B.Replace("下午", "PM").Replace("上午", "AM");
                        DateTime formattedReservationtime = DateTime.ParseExact(formattedReservationtime_B, "yyyy/M/d tt hh:mm:ss", CultureInfo.InvariantCulture);
                        string formattedEndtime_B = Endtime_B.ToString("yyyy-MM-dd HH:mm:ss");
                        DateTime formattedEndtime = DateTime.ParseExact(formattedEndtime_B, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
 
                        var reservation = _context.Appointment_record.FirstOrDefault(r => r.ReservationTime == formattedReservationtime
                                                                                            && r.Classroom_id == classroomid_B
                                                                                            && r.StartTime == formattedStartTime
                                                                                            && r.EndTime == formattedEndtime
                                                                                            && r.Student_id == studentid_B);
            
                        if (reservation != null)
                        {
                            reservation.StartTime = starttime;
                            reservation.EndTime = endtime;
                            reservation.Classroom_id = classroom;
                            reservation.State = 4; // 3改成修改待審核
                            _context.SaveChanges(); // 保存修改

                            ViewBag.ReservationSuccess = true; // 表示修改預約成功
                            var admins = _context2.member.Where(m => m.Level == "管理員").ToList();
                            foreach(var admin in admins)
                            {
                                // 現在你擁有了每位管理者的 member ID（adminId），你可以進行相應的操作
                                _lineNotifyService.SendNotify(admin.AccessToken, $"{student_class} {studentid_B}已修改《{classroom}》教室預約申請",11537,52002770);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error: Failed to find reservation for the given time.");
                            ViewBag.ReservationSuccess = false;
                        }
                    }
                    else
                    {
                        // 處理時間格式失敗
                        Console.WriteLine("Error: Invalid date/time format");
                        ViewBag.ReservationSuccess = false;
                    }
                }
                else
                {
                    //沒這個值
                    Console.WriteLine("Error: startTime_B is null or empty");
                    ViewBag.ReservationSuccess = false;
                }
            }
            else
            {
                Console.WriteLine("error");
            }






            return View();
        }
        ///我就問格式到底是想怎麼樣

        ///管理者頁面
        //審核預約 
        protected IPagedList<Appointment_record> GetPagedProcess_managementybook(int? page, int pageSize)
        {
            // 過濾從client傳送過來有問題頁數
            if (page.HasValue && page < 1)
            return null;
            // 從資料庫取得資料
            var listUnpaged = GetDatabase_managementbook();
            IPagedList<Appointment_record> pagelist = listUnpaged.ToPagedList(page ?? 1, pageSize);
            // 過濾從client傳送過來有問題頁數，包含判斷有問題的頁數邏輯
            if (pagelist.PageNumber != 1 && page.HasValue && page > pagelist.PageCount)
            return null;
            return pagelist;
        }
        protected IQueryable<Appointment_record> GetDatabase_managementbook()
        {
            DateTime today = DateTime.Now.Date;
            DateTime fourteenDaysLater = today.AddDays(14).Date;

            return _context.Appointment_record
            .Where(c => c.State != 1 && c.State != 2 && c.State != 3 && c.StartTime>=today)
                .OrderBy(c => c.StartTime)
                .ThenBy(c => c.Classroom_id)
                .ThenBy(c => c.ReservationTime);
        }
      
        
        [Route("ClassroomReserve/booked_classroom_management")]
        public async Task<IActionResult> booked_classroom_management(bool? ReservationSuccess ,int? page)
        {
            ViewBag.ReservationSuccess = ReservationSuccess;
            ViewBag.nowtime = currentTime;
            ViewBag.Date = currentTime.ToString("yyyy-MM-dd");
            DateTime today = DateTime.Now.Date;

            const int pageSize = 6;
            //處理頁數
            ViewBag.Appointment_record = GetPagedProcess_managementybook(page, pageSize);
            //填入頁面資料
            return View(await _context.Appointment_record.Where(c => c.State != 1 && c.State != 2 && c.State != 3 && c.StartTime>=today)
                .OrderBy(c => c.StartTime)
                .ThenBy(c => c.Classroom_id)
                .ThenBy(c => c.ReservationTime)
                .Skip(pageSize * ((page ?? 1) - 1))
                .Take(pageSize)
                .ToListAsync());
        }

        [HttpPost]
        [Route("ClassroomReserve/booked_classroom_management")]
        public async Task<IActionResult> booked_classroom_management()
        {
            try
            {
                var user = HttpContext.Session.GetString("User");
                var data = JsonConvert.DeserializeObject<Member>(user);
                var MemberId = data.id;//設定變數
                var MemberID = _context2.member.Find(MemberId);//去撈出id對應的member的資料
                
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync(); 
                    Console.WriteLine("body" + body);

                    int jsonStart = body.IndexOf("{");
                    if (jsonStart >= 0)
                    {
                        string jsonString = body.Substring(jsonStart);
                        var requestData = System.Text.Json.JsonSerializer.Deserialize<management_request>(jsonString);
                        Console.WriteLine("requestData.StartTime" + requestData.StartTime); 

                        if (requestData.Reason == null ) //沒理由沒規則沒刪除沒借用->審核成功
                        {
                            Console.WriteLine("have not reason");
                                
                            // 使用格式化后的字符串与数据库中的 DateTime 字段进行比较
                            var reservationToDelete1 = _context.Appointment_record
                            .AsEnumerable()
                                .Where(record => record.StartTime != null && record.EndTime != null &&
                                                record.StartTime == requestData.StartTime &&
                                                record.EndTime == requestData.EndTime && record.Classroom_id == requestData.Classroom_id&& record.Student_id == requestData.Student_id )
                                .ToList();

                            if (reservationToDelete1 != null)
                            {
                                foreach (var reservationTo1 in reservationToDelete1)
                                {
                                    reservationTo1.State = 1; // 表 審核成功
                                }
                                // Save changes to the database
                                await _context.SaveChangesAsync();
                                Console.WriteLine("Delete success!!!!!!!!!!!!!!!!!!!!");
                                
                                _lineNotifyService.SendNotify(MemberID.AccessToken, $"您《{requestData.Classroom_id}》教室預約申請審核成功",11537,52002740);
                                // 返回一個成功的消息
                                return Json(new { success = true, message = "預約同意" });
                            }
                            else
                            {
                                Console.WriteLine("Delete error: Reservation not found");
                                // 如果沒有找到相應的預約，返回一個失敗的消息
                                return Json(new { success = false, message = "找不到預約資料" });
                            }
                        }
                        else if (requestData.Reason != null ) //沒理由有規則沒刪除->修改規則
                        {
                            Console.WriteLine("have reason");
                            // 使用格式化后的字符串与数据库中的 DateTime 字段进行比较
                            var reservationToDelete1 = _context.Appointment_record
                            .AsEnumerable()
                            .Where(record => record.StartTime != null && record.EndTime != null &&
                                            record.StartTime == requestData.StartTime &&
                                            record.EndTime == requestData.EndTime && record.Classroom_id == requestData.Classroom_id&& record.Student_id == requestData.Student_id)
                            .ToList();

                            if (reservationToDelete1 != null)
                            {
                                foreach (var reservationTo1 in reservationToDelete1)
                                {
                                    reservationTo1.State = 2; // 表 已預約(預約失敗)
                                    reservationTo1.Reason = requestData.Reason; // 
                                }
                                // Save changes to the database
                                await _context.SaveChangesAsync();

                                Console.WriteLine("reasondata"+requestData.Reason);
                                Console.WriteLine("Delete success!");
                                // 返回一個成功的消息
                                _lineNotifyService.SendNotify(MemberID.AccessToken, $"您《{requestData.Classroom_id}》教室預約申請審核失敗,失敗理由為{requestData.Reason}。",11537,52002749);
                                return Json(new { success = true, message = "拒絕預約訊息已傳送" });
                            }
                        }
                        else{
                            return Json(new { success = false, message = "找不到資料" });
                        }
                        
                    return Json(new { success = false, message = "?" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "錯了" });
                    }
                }
            } 
            catch (DbUpdateException ex)
            {
                Console.WriteLine("Save changes failed: " + ex.Message);
                return Json(new { success = false, message = "錯誤 :" + ex.Message });
            } 
            
        }
        
        protected IPagedList<Appointment_record> GetPagedProcess(int? page, int pageSize)
        {
            // 過濾從client傳送過來有問題頁數
            if (page.HasValue && page < 1)
            return null;
            // 從資料庫取得資料
            var listUnpaged = GetStuffFromDatabase();
            IPagedList<Appointment_record> pagelist = listUnpaged.ToPagedList(page ?? 1, pageSize);
            // 過濾從client傳送過來有問題頁數，包含判斷有問題的頁數邏輯
            if (pagelist.PageNumber != 1 && page.HasValue && page > pagelist.PageCount)
            return null;
            return pagelist;
        }
        protected IQueryable<Appointment_record> GetStuffFromDatabase()
        {
            return _context.Appointment_record
            .OrderBy(c => c.StartTime)
            .ThenBy(c => c.Classroom_id)
            .ThenBy(c => c.ReservationTime);
        }
        //歷史資料
        [Route("ClassroomReserve/history_classroom_management")]
        public async Task<IActionResult> history_classroom_managementAsync(bool? ReservationSuccess,int? page = 1)
        {
        
            ViewBag.ReservationSuccess = ReservationSuccess;
            ViewBag.nowtime = currentTime;
            ViewBag.Date = currentTime.ToString("yyyy-MM-dd");
            DateTime today = DateTime.Now.Date;


            const int pageSize = 6;
            //處理頁數
            ViewBag.Appointment_record = GetPagedProcess(page, pageSize);
            
            return View(await _context.Appointment_record.OrderBy(c => c.StartTime)
                .ThenBy(c => c.Classroom_id)
                .ThenBy(c => c.ReservationTime)
                .Skip(pageSize * ((page ?? 1) - 1))
                .Take(pageSize)
                .ToListAsync());
           
        }
        protected IPagedList<borrow_key> GetPagedProcess_key(int? page, int pageSize)
        {
            // 過濾從client傳送過來有問題頁數
            if (page.HasValue && page < 1)
            return null;
            // 從資料庫取得資料
            var listUnpaged = GetStuffFromDatabase_key();
            IPagedList<borrow_key> pagelist = listUnpaged.ToPagedList(page ?? 1, pageSize);
            // 過濾從client傳送過來有問題頁數，包含判斷有問題的頁數邏輯
            if (pagelist.PageNumber != 1 && page.HasValue && page > pagelist.PageCount)
            return null;
            return pagelist;
        }
        protected IQueryable<borrow_key> GetStuffFromDatabase_key()
        {
            return _context.borrow_key
            .Where(c => c.return_time == null)
                .OrderBy(c => c.borrow_time) // 按照 borrow_time 的日期大小排序
                .ThenBy(c => c.Classroom_id);
        }
        [Route("ClassroomReserve/key_classroom_management")]
        public async Task<IActionResult> key_classroom_management(bool? ReservationSuccess,int? page = 1)
        {
        
            ViewBag.ReservationSuccess = ReservationSuccess;
            ViewBag.nowtime = currentTime;
            ViewBag.Date = currentTime.ToString("yyyy-MM-dd");
            DateTime today = DateTime.Now.Date;
          

            const int pageSize = 6;
            //處理頁數
            ViewBag.borrow_key = GetPagedProcess_key(page, pageSize);
            
            return View(await _context.borrow_key.Where(c => c.return_time == null)
                .OrderBy(c => c.borrow_time) // 按照 borrow_time 的日期大小排序
                .ThenBy(c => c.Classroom_id)
                .Skip(pageSize * ((page ?? 1) - 1))
                .Take(pageSize)
                .ToListAsync());
           
        }
       
        [HttpPost]
        [Route("ClassroomReserve/key_classroom_management")]
        public async Task<IActionResult> key_classroom_management()
        {
            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync(); 
                    Console.WriteLine("body" + body);

                    int jsonStart = body.IndexOf("{");
                    if (jsonStart >= 0)
                    {
                        string jsonString = body.Substring(jsonStart);
                        var requestData = System.Text.Json.JsonSerializer.Deserialize<management_request>(jsonString);
                  

                       
                        if (requestData.borrow != null)//沒理由沒規則沒刪除有借用日期->歸還鑰匙
                        {
                            Console.WriteLine("have reason");
                            // 使用格式化后的字符串与数据库中的 DateTime 字段进行比较
                            var reservationToDelete1 = _context.borrow_key
                            .FirstOrDefault(record => record.borrow_time != null  &&
                                            record.borrow_time == requestData.borrow_time &&
                                            record.Classroom_id == requestData.Classroom_id&&
                                            record.Student_id == requestData.Student_id
                                            );

                            if (reservationToDelete1 != null)
                            {
                                // 将 return_time 属性设置为当前时间
                                reservationToDelete1.return_time = DateTime.Now;

                                // 更新数据库中的记录
                                _context.borrow_key.Update(reservationToDelete1);
                                
                                // 保存更改
                                await _context.SaveChangesAsync();
                                var user = HttpContext.Session.GetString("User");
                                var data = JsonConvert.DeserializeObject<Member>(user);
                                var MemberId = data.id;//設定變數
                                var MemberID = _context2.member.Find(MemberId);//去撈出id對應的member的資料
                                _lineNotifyService.SendNotify(MemberID.AccessToken, $"您《{requestData.Classroom_id}》教室鑰匙已歸還成功。",11537,52002734);

                                // 返回成功消息
                                return Json(new { success = true, message = "資料庫已修改" });
                            }
                            else
                            {
                                Console.WriteLine("Delete error: Reservation not found");
                                // 如果沒有找到相應的預約，返回一個失敗的消息
                                return Json(new { success = false, message = "找不到資料" });
                            }
                        }
                        else{
                            return Json(new { success = false, message = "找不到資料" });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "錯了" });
                    }
                }
            } 
            catch (DbUpdateException ex)
            {
                Console.WriteLine("Save changes failed: " + ex.Message);
                return Json(new { success = false, message = "錯誤 :" + ex.Message });
            } 
            
        }

        protected IPagedList<borrow_key> GetPagedProcess_keyhistorym(int? page, int pageSize)
        {
            // 過濾從client傳送過來有問題頁數
            if (page.HasValue && page < 1)
            return null;
            // 從資料庫取得資料
            var listUnpaged = GetDatabase_keyhistorym();
            IPagedList<borrow_key> pagelist = listUnpaged.ToPagedList(page ?? 1, pageSize);
            // 過濾從client傳送過來有問題頁數，包含判斷有問題的頁數邏輯
            if (pagelist.PageNumber != 1 && page.HasValue && page > pagelist.PageCount)
            return null;
            return pagelist;
        }
        protected IQueryable<borrow_key> GetDatabase_keyhistorym()
        {
            return _context.borrow_key
                .OrderByDescending(c => c.return_time == null) // 將 return_time 是 null 的記錄排到前面
                .ThenBy(c => c.borrow_time)
                .ThenBy(c => c.Classroom_id);
        }
      
        
        [Route("ClassroomReserve/keyhistory_classroom_management")]
        public async Task<IActionResult> keyhistory_classroom_management(int? page)
        {
            ViewBag.nowtime = currentTime;
            const int pageSize = 6;
            //處理頁數
            ViewBag.borrow_key = GetPagedProcess_keyhistorym(page, pageSize);
            //填入頁面資料
            return View(await _context.borrow_key
                .OrderByDescending(c => c.return_time == null) // 將 return_time 是 null 的記錄排到前面
                .ThenBy(c => c.borrow_time)
                .ThenBy(c => c.Classroom_id)
                .Skip(pageSize * ((page ?? 1) - 1))
                .Take(pageSize)
                .ToListAsync());
        }
        //教室管理
        [Route("ClassroomReserve/interface_classroom_management")]
        public IActionResult interface_classroom_management(bool? ReservationSuccess)
        {
        
            ViewBag.ReservationSuccess = ReservationSuccess;
            ViewBag.nowtime = currentTime;
            ViewBag.Date = currentTime.ToString("yyyy-MM-dd");
            DateTime today = DateTime.Now.Date;
        

            var Classroom_information = _context.Classroom_information
                .OrderBy(c => c.Classroom_id)
                .ToList();

            // 返回ViewModel到View
            return View(Classroom_information);
        }

        [HttpPost]
        [Route("ClassroomReserve/interface_classroom_management")]
        public async Task<IActionResult> interface_classroom_management(string ruleData)
        {
            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync(); 
                    Console.WriteLine("body" + body);

                    int jsonStart = body.IndexOf("{");
                    if (jsonStart >= 0)
                    {
                        string jsonString = body.Substring(jsonStart);
                        var requestData = System.Text.Json.JsonSerializer.Deserialize<management_request>(jsonString);

                        if ( requestData.deleteClassroom != null  )//沒理由沒規則有刪除->刪除資訊
                        {
                        
                            // 使用格式化后的字符串与数据库中的 DateTime 字段进行比较
                            var reservationToDelete1 = _context.Classroom_information
                            .FirstOrDefault(record => record.Classroom_id == requestData.Classroom_id);

                            if (reservationToDelete1 != null)
                            {
                                // 刪除資料庫中的舊圖片
                                if (!string.IsNullOrEmpty(reservationToDelete1.Path))
                                {
                                    // 請根據您的資料庫模型和刪除方法進行修改
                                    // 這只是一個示例，您需要根據您的實際情況進行修改
                                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "./wwwroot/images", reservationToDelete1.Path);
                                    if (System.IO.File.Exists(oldImagePath))
                                    {
                                        System.IO.File.Delete(oldImagePath);
                                    }
                                }
                                reservationToDelete1.Path = null;
                                _context.Classroom_information.Remove(reservationToDelete1); // 从数据库中删除记录
                                // Save changes to the database
                                await _context.SaveChangesAsync();

                            
                                // 返回一個成功的消息
                                return Json(new { success = true, message = "資料庫已修改" });
                            }
                            else
                            {
                                Console.WriteLine("Delete error: Reservation not found");
                                // 如果沒有找到相應的預約，返回一個失敗的消息
                                return Json(new { success = false, message = "找不到資料" });
                            }
                        }
                       
                        else{
                            return Json(new { success = false, message = "找不到資料" });
                        }
                    }
                    
                    else
                    {
                        return Json(new { success = false, message = "錯了" });
                    }
                }
            } 
            catch (DbUpdateException ex)
            {
                Console.WriteLine("Save changes failed: " + ex.Message);
                return Json(new { success = false, message = "錯誤 :" + ex.Message });
            } 
            
        }
        //規則修改
        [Route("ClassroomReserve/rule__classroom_management")]
        public IActionResult rule__classroom_management(bool? ReservationSuccess)
        {
        
            ViewBag.ReservationSuccess = ReservationSuccess;
            ViewBag.nowtime = currentTime;
            ViewBag.Date = currentTime.ToString("yyyy-MM-dd");
            DateTime today = DateTime.Now.Date;
      

            //var maxId = _context.usage_rules.Max(ur => ur.rules_id);
            var usage_rules = _context.usage_rules
                .OrderByDescending(r => r.modify_time) // 按照時間列進行遞減排序
                .ToList(); // 轉換為列表

            if (usage_rules.Any())
            {
                var latestUsageRule = usage_rules.First(); 
                return View(new List<usage_rules> { latestUsageRule }); // 將單個對象放入列表中傳遞給視圖
            }
            else
            {
                // 如果列表為空，傳遞一個空的模型到前端
                 return View(new List<usage_rules>());
            }

            // 返回ViewModel到View
            //return View(latestUsageRule);
        }
    


        [HttpPost]
        [Route("ClassroomReserve/rule__classroom_management")]
        public async Task<IActionResult> rule__classroom_management(string ruleData)
        {
            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync(); 
                    Console.WriteLine("body" + body);

                    int jsonStart = body.IndexOf("{");
                    if (jsonStart >= 0)
                    {
                        string jsonString = body.Substring(jsonStart);
                        var requestData = System.Text.Json.JsonSerializer.Deserialize<management_request>(jsonString);
                    

                        
                        if (requestData.RuleData != null )//有理由沒規則沒刪除->拒絕審核
                        {
                            Console.WriteLine("RuleData");
                            var usageRule = _context.usage_rules.FirstOrDefault();
                            DateTime nowtime = DateTime.Now;
                            Console.WriteLine("usageRule"+usageRule);
                            var newusage_rules = new usage_rules
                            {
                                rules = requestData.RuleData,
                                modify_time = nowtime
                            };

                            _context.Add(newusage_rules);
                
                            int savedChanges = _context.SaveChanges();
                            return Json(new { success = true, message = "修改已完成" });  
                        }
                        else{
                            return Json(new { success = false, message = "找不到資料" });
                        }
                        
                  
                    }
                    
                    else
                    {
                        Console.WriteLine("whereeeeeeeeeeeeeeeeeeeeeeee");
                        return Json(new { success = false, message = "錯了" });
                    }

                }
       
            } 
            catch (DbUpdateException ex)
            {
                Console.WriteLine("Save changes failed: " + ex.Message);
                return Json(new { success = false, message = "錯誤 :" + ex.Message });
            } 
            
        }

        [Route("ClassroomReserve/editClassroom")]
        public IActionResult editClassroom()
        {
            ViewBag.nowtime = currentTime;
            ViewBag.Date = currentTime.ToString("yyyy-MM-dd");

            return View();
        }

        [HttpPost]
        [Route("ClassroomReserve/editClassroom")]
        public async Task<IActionResult> editClassroom(IFormCollection formData)
        {
            ViewBag.nowtime = currentTime;
            ViewBag.Date = currentTime.ToString("yyyy-MM-dd");
            var classroomid = formData["classroomid"].ToString();
            var Equipment = formData["Equipment"].ToString();
            var Capacity = formData["Capacity"].ToString();
            var remark = formData["remark"].ToString();
            var selectOption = formData["selectOption"].ToString();
            //舊教室名稱
            var Classroomid_B = formData["Classroomid_B"].ToString();
            // 获取上传的照片文件
            var photo = formData.Files.GetFile("photo");
             
            var reservation = _context.Classroom_information
                .FirstOrDefault(r => r.Classroom_id == classroomid);

            if (reservation != null)
            {
                reservation.Classroom_id = classroomid;
                reservation.Equipment = Equipment;
                reservation.Capacity = Capacity;
                reservation.remark = remark; // 3改成修改待審核
                if (int.TryParse(selectOption, out int state))
                {
                    // 将状态值赋给 reservation.Classroom_state
                    reservation.Classroom_state = (byte?)state;
                }
                else
                {
                    // 如果无法成功转换，可以根据需要进行错误处理
                    // 例如，记录错误或提供默认值
                    Console.WriteLine("Failed to convert selectOption to int.");
                }
                //取得圖片檔名
                string GetImageType(string fileName)
                {
                    string extension = Path.GetExtension(fileName).ToLower();

                    switch (extension)
                    {
                        case ".jpg":
                        case ".jpeg":
                            return "jpg";
                        case ".png":
                            return "png";
                        case ".gif":
                            return "gif";
                        case ".bmp":
                            return "bmp";
                        default:
                            return "Unknown";
                    }
                }
                
                if (photo != null && photo.Length > 0)
                {
                    string imageType = GetImageType(photo.FileName);
                    // 刪除資料庫中的舊圖片
                    if (!string.IsNullOrEmpty(reservation.Path))
                    {
                        // 請根據您的資料庫模型和刪除方法進行修改
                        // 這只是一個示例，您需要根據您的實際情況進行修改
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "./wwwroot/images", reservation.Path);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    var newFileName = $"classroomid_{classroomid}.{imageType}";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "./wwwroot/images", newFileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream);
                    }
                   
                    reservation.Path = null;

                    // 更新 reservation 的 Path 屬性
                    reservation.Path = newFileName;
                }

                _context.SaveChanges(); // 保存修改

                ViewBag.ReservationSuccess = true; // 表示修改預約成功
                Console.WriteLine("update data.");

            }
            else
            {
                Console.WriteLine("Error: Failed to find reservation for the given time.");
                ViewBag.ReservationSuccess = false;
            }

            return View();
        }


        [Route("ClassroomReserve/createClassroom")]
        public IActionResult createClassroom()
        {
            ViewBag.nowtime = currentTime;
            ViewBag.Date = currentTime.ToString("yyyy-MM-dd");

            return View();
        }

        [HttpPost]
        [Route("ClassroomReserve/createClassroom")]
        public async Task<IActionResult> CreateClassroom(IFormCollection formData)
        {
            // 獲取數據
            var classroomId = formData["classroomid"].ToString();
            var equipment = formData["Equipment"].ToString();
            var capacity = formData["Capacity"].ToString();
            var remark = formData["remark"].ToString();
            var selectOption = formData["selectOption"].ToString();
            var photo = formData.Files.GetFile("photo");

            // 新增資料進資料庫
            var newClassroom = new Classroom_information
            {
                Classroom_id = classroomId,
                Equipment = equipment,
                Capacity = capacity,
                remark = remark,
                Classroom_state = byte.TryParse(selectOption, out byte state) ? state : (byte)0
            };
            string GetImageType(string fileName)
            {
                string extension = Path.GetExtension(fileName).ToLower();

                switch (extension)
                {
                    case ".jpg":
                    case ".jpeg":
                        return "jpg";
                    case ".png":
                        return "png";
                    case ".gif":
                        return "gif";
                    case ".bmp":
                        return "bmp";
                    default:
                        return "Unknown";
                }
            }
            

            // 有上傳照片 保存並傳到資料夾
            if (photo != null && photo.Length > 0)
            {
                string imageType = GetImageType(photo.FileName);
                var newFileName = $"classroomid_{classroomId}.{imageType}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "./wwwroot/images", newFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

            
                //設置照片檔名到資料庫
                newClassroom.Path = newFileName;
            }

            // 添加到資料庫並保存
            _context.Classroom_information.Add(newClassroom);
            await _context.SaveChangesAsync();

            // 表創建成功
            ViewBag.ReservationSuccess = true;

            return View();
        }

        
        [Route("ClassroomReserve/borrow_key")]
        public IActionResult borrow_key()
        {
            ViewBag.Message = "借用鑰匙";
            ViewBag.nowtime = currentTime;
            ViewBag.Date = currentTime.ToString("yyyy-MM-dd");
            return View();
        }

        [HttpPost]
        [Route("ClassroomReserve/borrow_key")]
        public async Task<IActionResult> borrow_key(IFormCollection formData)
        {
            // 取得數據
            var classroomId = formData["classroom"].ToString();
            var borrowtime = formData["borrowtime"].ToString();
 
            //抓到登入者訊息
            var user = HttpContext.Session.GetString("User");
            var data = JsonConvert.DeserializeObject<Member>(user);
            var admins = _context2.member.Where(m => m.Level == "管理員").ToList();
            try
            {
                if (!string.IsNullOrEmpty(borrowtime))
                {
                    // Parse strings to DateTime
                    if (DateTime.TryParse(borrowtime, out DateTime Borrowtime) )
                    {
                        // Add new records to the database
                        var newborrow_key = new borrow_key
                        {
                            borrow_time = Borrowtime,
                            Classroom_id = classroomId,
                            Student_id = data.studentID,
                            Class = data.student_class, 
                            Student_name = data.student_name,
                        };
                        _context.Add(newborrow_key);
             
                        int savedChanges = _context.SaveChanges();
                        ViewBag.ReservationSuccess = true;
                        foreach(var admin in admins)
                        {
                            // 現在你擁有了每位管理者的 member ID（adminId），你可以進行相應的操作
                            _lineNotifyService.SendNotify(admin.AccessToken, $"{data.student_class} {data.studentID}已填寫《{classroomId}》教室鑰匙借用表單",11537,52002739);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error: Failed to parse starttime or endTime.");
                        ViewBag.ReservationSuccess = false;
                    }
                }
                else
                {
                    Console.WriteLine("Error: starttime or endTime is null or empty.");
                    ViewBag.ReservationSuccess = false;
                }
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                ViewBag.ReservationSuccess = false;
                ViewBag.ErrorMessage = "An error occurred: " + ex.Message;
            }

            return View();
        }

        

    }
}





