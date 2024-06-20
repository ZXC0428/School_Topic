using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;// 引入 Entity Framework Core 命名空間
using TA.ViewModels;
using Student_web.Services;
using Student_web.Models;
using Student_web.Data;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace TA.Controllers
{
    public class TAReserveController : Controller
    {
        private readonly TADbContext _context;
        private readonly ILineNotifyService _lineNotifyService;
        private readonly ApplicationDbContext _context2;
        private readonly ILogger<TAReserveController> _logger;
        private readonly WordDocumentGenerator _wordDocumentGenerator;

        public TAReserveController(ILogger<TAReserveController> logger, TADbContext context,ILineNotifyService lineNotifyService, ApplicationDbContext context2, WordDocumentGenerator wordDocumentGenerator) // 添加 MyDbContext 作為參數
        {
            _logger = logger;
            _context = context; // 初始化 MyDbContext
            _lineNotifyService = lineNotifyService;
            _context2 = context2;
            _wordDocumentGenerator = wordDocumentGenerator;
        }
        
        public IActionResult TATables()
        {
            try
            {
                var today = DateTime.Today;
                var TATables = _context.TATables.Where(t => t.Date >= today).OrderBy(t => t.Startime).ToList();
                ViewBag.Message = "本週課輔";
                return View(TATables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in TATables action");
                throw;
            }
        }

        public IActionResult AddCourse()
        {
            return View(new TATable()); //初始化一个新的TATable对象以供添加课程时使用
        }

        [HttpPost]
        public async Task<IActionResult> AddCourse(TATable course, DateTime endDate)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Starting AddCourse operation.");
                    var currentDate = course.Date;
                    while (currentDate <= endDate)
                    {
                        var newCourse = new TATable
                        {
                            CourseName = course.CourseName,
                            Date = currentDate,
                            Startime = course.Startime,
                            Overtime = course.Overtime,
                            TAName = course.TAName,
                            Classroom = course.Classroom,
                            Ta_id = course.Ta_id // 將Ta_id設置為從表單中接收到的值
                        };
                        _context.TATables.Add(newCourse);
                        // 將當前日期增加一週
                        currentDate = currentDate?.AddDays(7);
                    }
                    // 日志记录：成功添加课程
                    _logger.LogInformation("Course added successfully.");
                    await _context.SaveChangesAsync();
                    return RedirectToAction("TATables"); // 假设您有一个显示课程列表的视图
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding course");
                    return View(course);
                }
            }
            return View(course);
        }


        
        // 在此处编写用于取消预约的操作
        [HttpPost]
        // 在 TAReserveController 中添加取消预约的动作方法
        public IActionResult CancelReservation(int reservationId)
        {
            try
            {
                // 查找要取消的预约
                var reservation = _context.TAreserves.FirstOrDefault(r => r.Tacourse_id == reservationId);

                // 嘗試找到該用戶的資料以發送通知
                //抓到登入者訊息
                var tatble_id = _context.TATables.FirstOrDefault(t => t.Tacourse_id == reservationId);
                var TA_ID = tatble_id?.Ta_id;
                var st_id = reservation?.Student_id;
                var st_name = _context2.member.FirstOrDefault(m => m.studentID == st_id);
                var coursesday = tatble_id?.Date?.ToString("yyyy-MM-dd");
                var coursename = tatble_id?.CourseName;

                var MemberID = _context2.member.FirstOrDefault(m => m.studentID == TA_ID);
                
                // 记录要取消的预约ID
                _logger.LogInformation("Attempting to cancel reservation with ID: {ReservationId}", reservationId);

                // 如果找到了预约，则从数据库中移除它
                if (reservation != null)
                {
                    _context.TAreserves.Remove(reservation);
                    _context.SaveChanges();

                    if (MemberID.AccessToken != null)
                    {
                        _lineNotifyService.SendNotify(MemberID.AccessToken, $"{st_id} {st_name.student_name} 取消預約 {coursesday} 的 {coursename}",6632,11825381);
                    }

                    
                    // 取消预约成功，重定向到适当的视图或操作
                    _logger.LogInformation("Reservation with ID {ReservationId} canceled successfully", reservationId);
                    return RedirectToAction("TAreserves", "TAReserve"); // 假设 Index 是显示预约列表的视图

                }
                else
                {
                    // 如果未找到预约，返回错误信息
                    _logger.LogWarning("Reservation with ID {ReservationId} not found", reservationId);
                    return NotFound("Reservation not found");
                }
            }
            catch (Exception ex)
            {
                // 如果取消预约过程中发生异常，记录错误并返回错误信息
                _logger.LogError(ex, "Error canceling reservation");
                return StatusCode(500, "An error occurred while canceling the reservation. Please try again later.");
            }
        }


        //取消預約頁面
        public IActionResult TAreserves()
        {
            var userInput = HttpContext.Session.GetString("User");
            dynamic user = null;
            if (!string.IsNullOrEmpty(userInput))
            {
                user = JsonConvert.DeserializeObject<dynamic>(userInput);
            }

            if (user == null)
            {
                return View("Error");
            }

            var UserStudentId = (string)user.studentID;

            var reservedCourses = (from r in _context.TAreserves
                                join t in _context.TATables on r.Tacourse_id equals t.Tacourse_id
                                where r.Student_id == UserStudentId
                                select new ReservedCourseViewModel
                                {
                                    CourseName = t.CourseName,
                                    TAName = t.TAName,
                                    ReservationId = t.Tacourse_id,
                                    Reservestime = (DateTime)t.Date,
                                    HasFeedback = _context.Feedbacks.Any(f => f.Tacourse_id == t.Tacourse_id && f.studentID == UserStudentId)
                                }).ToList();

            return View(reservedCourses);
        }

        // 假設 TAreserves 是處理表單提交的方法
        [HttpPost]
        public IActionResult TAreserves1(int course_Id)
        {
            var courseInfo = _context.TAreserves.FirstOrDefault(c => c.Tacourse_id == course_Id);
            Console.WriteLine(courseInfo);
            if (courseInfo == null)
            {
                return NotFound("Course not found");
            }

            var schedule = new SchedulesModel
            {
                CourseID = courseInfo.Tacourse_id,
                St_id = courseInfo.Student_id
            };

            return RedirectToAction("Feedbacks", schedule); // 将 schedule 实例传递给 Feedbacks 视图
        }


        //Feedback
        public IActionResult Feedbacks(SchedulesModel schedules)
        {
            if (schedules == null)
            {
                return NotFound("Schedules data is not provided.");
            }

            // 直接從傳入的 schedules 實例中獲取 CourseID
            var courseId = schedules.CourseID;
            ViewBag.CourseID = courseId;

            var feedbackModel = new Feedback
            {
                Tacourse_id = courseId // 假設 TA.Feedback 有一個 Tacourse_id 屬性來存儲課程ID
            };

            // 將 feedbackModel 作為模型傳遞給視圖
            return View(feedbackModel);
        }


        //Add Feedback
        [HttpPost]
        public IActionResult Feedbacks(Feedback feedback, int rating, int question1, int question2, int question3, int question4, string Student_id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 將用戶的反饋內容和助教滿意度評分保存到數據庫中
                    feedback.answer_1 = question1;
                    feedback.answer_2 = question2;
                    feedback.answer_3 = question3;
                    feedback.answer_4 = question4;
                    feedback.answer_5 = rating;
                    feedback.studentID = Student_id;
                    
                    _context.Feedbacks.Add(feedback);
                    _context.SaveChanges();

                    // 返回成功消息或者重定向到其他頁面
                    return RedirectToAction("TAreserves"); // 重定向到 TAreserves 操作
                }
                catch (Exception ex)
                {
                    // 處理保存失敗的情況
                    ModelState.AddModelError("", "Failed to submit feedback. Please try again later.");
                    _logger.LogError(ex, "Error adding feedback");
                    return View(feedback);
                }
            }

            // 如果 ModelState 驗證失敗，則返回帶有錯誤消息的當前視圖
            return View(feedback);
        }




        //Courseeditor
        // GET: 显示课程列表视图
        public IActionResult Courseeditor(CoursesViewModel filterModel)
        {
            var courses = _context.TATables.AsQueryable();

            // 根据过滤条件查询课程
            if (filterModel.StartDate.HasValue && filterModel.EndDate.HasValue)
            {
                courses = courses.Where(c => c.Date >= filterModel.StartDate && c.Date <= filterModel.EndDate);
            }

            if (!string.IsNullOrEmpty(filterModel.CourseName))
            {
                courses = courses.Where(c => c.CourseName.Contains(filterModel.CourseName));
            }

            // 构建视图模型
            var viewModel = new CoursesViewModel
            {
                StartDate = filterModel.StartDate,
                EndDate = filterModel.EndDate,
                CourseName = filterModel.CourseName,
                Courses = courses.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Edit(TATable course)
        {
            if (ModelState.IsValid)
            {
                _context.Update(course);
                _context.SaveChanges();
                return RedirectToAction("Courseeditor"); // 编辑成功后重定向到课程列表页面
            }
            return View(course); // 如果模型验证失败，返回编辑视图
        }

        // POST: 处理课程编辑的提交
        [HttpPost]
        public async Task<IActionResult> UpdateCourse([FromBody] TATable updatedCourse)
        {

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for updating course.");
                return BadRequest(ModelState);
            }

            try
            {
                var existingCourse = await _context.TATables
                    .FirstOrDefaultAsync(c => c.Tacourse_id == updatedCourse.Tacourse_id);

                if (existingCourse == null)
                {
                    _logger.LogWarning($"Course with ID {updatedCourse.Tacourse_id} not found for update.");
                    return NotFound("Course not found");
                }

                // 更新课程信息
                existingCourse.CourseName = updatedCourse.CourseName;
                existingCourse.Date = updatedCourse.Date;
                existingCourse.Startime = updatedCourse.Startime;
                existingCourse.Overtime = updatedCourse.Overtime;
                existingCourse.TAName = updatedCourse.TAName;
                existingCourse.Classroom = updatedCourse.Classroom;
                existingCourse.Ta_id = updatedCourse.Ta_id;

                _context.Update(existingCourse);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Course with ID {updatedCourse.Tacourse_id} updated successfully.");
                return Ok("Course updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating course with ID {updatedCourse.Tacourse_id}.");
                return StatusCode(500, "Error updating course");
            }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var courseToDelete = _context.TATables.FirstOrDefault(c => c.Tacourse_id == id);
            _context.TATables.Remove(courseToDelete);
            _context.SaveChanges();
            return Json(new { success = true });
        }

        public ActionResult Reedfeedback()
        {
            // 获取所有反馈数据
            var feedbackList = _context.Feedbacks.ToList();

            // 预先加载所有相关的 TATables 数据，以便后续处理
            var courseIds = feedbackList.Select(f => f.Tacourse_id).Distinct().ToList();
            var courses = _context.TATables.Where(t => courseIds.Contains(t.Tacourse_id)).ToList();

            // 构建 ViewModel 列表
            var feedbackViewModelList = feedbackList.Select(feedback => {
                var course = courses.FirstOrDefault(t => t.Tacourse_id == feedback.Tacourse_id);
                return new FeedbackViewModel
                {
                    CourseName = course?.CourseName ?? "未知课程",
                    TAName = course?.TAName ?? "未知 TA",
                    Txt = feedback.Txt,
                    answer_1 = feedback.answer_1,
                    answer_2 = feedback.answer_2,
                    answer_3 = feedback.answer_3,
                    answer_4 = feedback.answer_4,
                    answer_5 = feedback.answer_5
                };
            }).ToList();

            // 将视图模型传递给视图
            return View(feedbackViewModelList);
        }

        public IActionResult TASchedules()
        {
            var allCourses = _context.TATables.ToList();
            var pastCourses = new List<ReservedCourseViewMode2>();

            foreach (var course in allCourses)
            {
                // 檢查課程日期是否小於當前日期
                if (course.Date < DateTime.Now)
                {
                    pastCourses.Add(new ReservedCourseViewMode2
                    {
                        CourseName = course.CourseName,
                        TAName = course.TAName,
                        ReservationId = course.Tacourse_id,
                        CourseDay = (DateTime)course.Date,
                        TA_ID = course.Ta_id,
                    });
                }
            }

            return View(pastCourses); // 返回包含過去課程的視圖
        }

        public IActionResult Appointmentrecord(int reservationid)
        {
            var course = _context.TATables.FirstOrDefault(t => t.Tacourse_id == reservationid);
            if (course == null)
            {
                return View("Error"); // 或其他錯誤處理方式
            }

            var studentReserves = _context.TAreserves.Where(r => r.Tacourse_id == reservationid).ToList();
            var students = studentReserves.Select(r => new {
                StudentId = r.Student_id,
                StudentName = _context2.member.FirstOrDefault(m => m.studentID == r.Student_id)?.student_name
            }).ToList();

            var model = new CourseViewModel
            {
                CourseDetails = new CourseDetailsModel
                {
                    CourseID = course.Tacourse_id,
                    Day = course.Date,
                    StartTime = course.Startime,
                    Overtime = course.Overtime,
                    Coursename = course.CourseName,
                    TA_ID = course.Ta_id,
                    Taname = course.TAName,
                    Classroom = course.Classroom
                },
                Students = students.Select(s => new StudentModel
                {
                    StudentId = s.StudentId,
                    StudentName = s.StudentName
                }).ToList()
            };

            return View(model);
        }

        private TATable GetCourseDetails(int reservationId)
        {
            var course = _context.TATables.FirstOrDefault(t => t.Tacourse_id == reservationId);
            return course;
        }

        //EXCEL
        public IActionResult ExportToExcel(int reservationid)
        {
            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Course Details");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "日期";
                worksheet.Cell(currentRow, 2).Value = "開始時間";
                worksheet.Cell(currentRow, 3).Value = "結束時間";
                worksheet.Cell(currentRow, 4).Value = "課程名稱";
                worksheet.Cell(currentRow, 5).Value = "TA ID";
                worksheet.Cell(currentRow, 6).Value = "TA 姓名";
                worksheet.Cell(currentRow, 7).Value = "教室";

                var course = GetCourseDetails(reservationid);
                if (course == null) return NotFound("Course not found");

                currentRow++;
                worksheet.Cell(currentRow, 1).Value = course.Date;
                worksheet.Cell(currentRow, 2).Value = course.Startime;
                worksheet.Cell(currentRow, 3).Value = course.Overtime;
                worksheet.Cell(currentRow, 4).Value = course.CourseName;
                worksheet.Cell(currentRow, 5).Value = course.Ta_id;
                worksheet.Cell(currentRow, 6).Value = course.TAName;
                worksheet.Cell(currentRow, 7).Value = course.Classroom;

                // 新增一個工作表來輸出學生名單
                var studentWorksheet = workbook.Worksheets.Add("Student List");
                studentWorksheet.Cell(1, 1).Value = "學號";
                studentWorksheet.Cell(1, 2).Value = "姓名";

                var studentReserves = _context.TAreserves.Where(r => r.Tacourse_id == reservationid).ToList();
                var row = 2;
                foreach (var reserve in studentReserves)
                {
                    var student = _context2.member.FirstOrDefault(m => m.studentID == reserve.Student_id);
                    studentWorksheet.Cell(row, 1).Value = student?.studentID ?? "未知";
                    studentWorksheet.Cell(row, 2).Value = student?.student_name ?? "未知";
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CourseDetails.xlsx");
                }
            }
        }
        //WORD
        public class WordDocumentGenerator
        {
            public void GenerateCourseDocument(string filePath, CourseViewModel model)
            {
                using (WordprocessingDocument doc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
                {
                    // Add a main document part
                    MainDocumentPart mainPart = doc.AddMainDocumentPart();
                    mainPart.Document = new Document();

                    // Add content to the document
                    Body body = mainPart.Document.AppendChild(new Body());

                    // Add heading
                    Paragraph heading = new Paragraph(new Run(new Text("課程資訊")));
                    body.AppendChild(heading);

                    // Add course details table
                    Table courseTable = new Table();
                    TableProperties courseTableProperties = new TableProperties(new TableLayout { Type = TableLayoutValues.Fixed });
                    courseTable.AppendChild(courseTableProperties);

                    // Add rows and cells for course details
                    AddRow(courseTable, "日期", model.CourseDetails?.Day?.ToString("yyyy/MM/dd"));
                    AddRow(courseTable, "開始時間", model.CourseDetails?.StartTime?.ToString(@"hh\:mm"));
                    AddRow(courseTable, "結束時間", model.CourseDetails?.Overtime?.ToString(@"hh\:mm"));
                    AddRow(courseTable, "課程名稱", model.CourseDetails?.Coursename);
                    AddRow(courseTable, "TAID", model.CourseDetails?.TA_ID);
                    AddRow(courseTable, "TA姓名", model.CourseDetails?.Taname);
                    AddRow(courseTable, "教室", model.CourseDetails?.Classroom);

                    body.AppendChild(courseTable);

                    // Add student list heading
                    Paragraph studentHeading = new Paragraph(new Run(new Text("學生名單")));
                    body.AppendChild(studentHeading);

                    // Add student list table
                    Table studentTable = new Table();
                    TableProperties studentTableProperties = new TableProperties(new TableLayout { Type = TableLayoutValues.Fixed });
                    studentTable.AppendChild(studentTableProperties);

                    // Add table headers
                    TableRow headerRow = new TableRow();
                    headerRow.AppendChild(new TableCell(new Paragraph(new Run(new Text("學號")))));
                    headerRow.AppendChild(new TableCell(new Paragraph(new Run(new Text("姓名")))));
                    studentTable.AppendChild(headerRow);

                    // Add rows for each student
                    foreach (var student in model.Students)
                    {
                        TableRow studentRow = new TableRow();
                        studentRow.AppendChild(new TableCell(new Paragraph(new Run(new Text(student.StudentId)))));
                        studentRow.AppendChild(new TableCell(new Paragraph(new Run(new Text(student.StudentName)))));
                        studentTable.AppendChild(studentRow);
                    }

                    body.AppendChild(studentTable);
                }
            }

            private void AddRow(Table table, string headerText, string cellText)
            {
                TableRow row = new TableRow();
                row.AppendChild(new TableCell(new Paragraph(new Run(new Text(headerText)))));
                row.AppendChild(new TableCell(new Paragraph(new Run(new Text(cellText)))));
                table.AppendChild(row);
            }
        }


        public IActionResult DownloadCourseDocument(int reservationid)
        {
            var course = _context.TATables.FirstOrDefault(t => t.Tacourse_id == reservationid);
            if (course == null)
            {
                return NotFound(); // 或其他错误处理方式
            }

            var studentReserves = _context.TAreserves.Where(r => r.Tacourse_id == reservationid).ToList();
            var students = studentReserves.Select(r => new {
                StudentId = r.Student_id,
                StudentName = _context2.member.FirstOrDefault(m => m.studentID == r.Student_id)?.student_name
            }).ToList();

            var model = new CourseViewModel
            {
                CourseDetails = new CourseDetailsModel
                {
                    CourseID = course.Tacourse_id,
                    Day = course.Date,
                    StartTime = course.Startime,
                    Overtime = course.Overtime,
                    Coursename = course.CourseName,
                    TA_ID = course.Ta_id,
                    Taname = course.TAName,
                    Classroom = course.Classroom
                },
                Students = students.Select(s => new StudentModel
                {
                    StudentId = s.StudentId,
                    StudentName = s.StudentName
                }).ToList()
            };

            // Generate Word document
            string filePath = "Path to save the Word document"; // 设置生成的Word文档保存的路径
            _wordDocumentGenerator.GenerateCourseDocument(filePath, model);

            // Return the file for download
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "CourseDocument.docx");
        }




    }

    

    [ApiController]
    [Route("api/[controller]")]
    public class DatesController : ControllerBase
    {
        private readonly TADbContext _context;

        public DatesController(TADbContext context)
        {
            _context = context;
        }

        // GET: api/Dates/{courseName}
        [HttpGet("{courseName}")]
        public async Task<IActionResult> GetDatesByCourseName(string courseName)
            {
            var dates = await _context.TATables
                .Where(t => t.CourseName.ToLower() == courseName.ToLower()) // 改寫為不使用 StringComparison
                .Select(t => t.Date.Value.Date.ToString("yyyy-MM-dd")) // 確保這裡進行了適當的null檢查和格式化
                .Distinct()
                .ToListAsync();

            if (!dates.Any())
            {
                return NotFound(new { message = "No dates found for the given course name." });
            }

            return Ok(dates);
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ReserveController : ControllerBase
    {
        private readonly TADbContext _context;
        private readonly ILogger<ReserveController> _logger;
        private readonly ILineNotifyService _lineNotifyService;
        private readonly ApplicationDbContext _context2;

        public ReserveController(TADbContext context, ILogger<ReserveController> logger, ILineNotifyService lineNotifyService,ApplicationDbContext context2)
        {
            _context = context;
            _logger = logger;
            _lineNotifyService = lineNotifyService;
            _context2 = context2;
        }

        [HttpPost]
        public async Task<IActionResult> ReserveCourse(ReserveRequest request)
        {
            _logger.LogInformation("Starting ReserveCourse operation for {CourseName} on {Date}", request.CourseName, request.Date);
            try
            {
                // 檢查是否已經有相同學號和課程ID的預約
                var course = await _context.TATables
                    .FirstOrDefaultAsync(t => t.CourseName == request.CourseName && t.Date == request.Date);
                bool alreadyReserved = await _context.TAreserves
                    .AnyAsync(r => r.Student_id == request.UserId && r.TATable.Tacourse_id == course.Tacourse_id);
                
                if (alreadyReserved)
                {
                    _logger.LogWarning("Duplicate reservation attempt by user {UserId} for course {CourseName} on {Date}", request.UserId, request.CourseName, request.Date);
                    // 返回 HTTP 409 状态码，同时发送包含错误信息的 JSON 对象
                    return Ok(new { message = "請勿重複預約相同課程！" });
                }
                else
                {
                    if (course == null)
                    {
                        _logger.LogWarning("No course found for {CourseName} on {Date}", request.CourseName, request.Date);
                        return BadRequest(new { message = "指定的課程和日期不存在。" });
                    }

                    if (string.IsNullOrEmpty(request.UserId))
                    {
                        _logger.LogWarning("User ID is not provided in the request.");
                        return BadRequest(new { message = "未提供用户ID。" });
                    }

                    var reservation = new TAreserve
                    {
                        Tacourse_id = course.Tacourse_id,
                        Student_id = request.UserId,
                        Reservestime = DateTime.Now
                    };

                    _context.TAreserves.Add(reservation);
                    await _context.SaveChangesAsync();

                    // 嘗試找到該用戶的資料以發送通知
                    //抓到登入者訊息
                    var TA_ID = course.Ta_id;
                    var st_id = request.UserId;
                    var st_name = _context2.member.FirstOrDefault(m => m.studentID == st_id);
                    var coursesday = course.Date?.ToString("yyyy-MM-dd");
                    var coursename = course.CourseName;

                    var MemberID = _context2.member.FirstOrDefault(m => m.studentID == TA_ID);
                    Console.WriteLine(st_id);
                    Console.WriteLine(st_name.student_name);
                    if (MemberID.AccessToken != null)
                    {
                        _lineNotifyService.SendNotify(MemberID.AccessToken, $"{st_id} {st_name.student_name} 預約 {coursesday} 的 {coursename}",8525,16581300);
                    }

                    return Ok(new { message = "預約成功！" });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ReserveCourse operation");
                return StatusCode(500, new {  message = "發生錯誤：" + ex.Message});
            }
        }

    }



}
