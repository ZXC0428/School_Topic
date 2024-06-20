using Microsoft.AspNetCore.Mvc;
using Student_web.Data;
using Student_web.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace Student_web.Controllers
{
    public class MemberController : Controller
    {
        private readonly ApplicationDbContext _context;
        public MemberController(ApplicationDbContext context)
        {
            _context = context;
        }      
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                //比對帳號密碼
                var user = _context.member.FirstOrDefault(u => u.studentID == model.member.studentID && u.password == model.member.password);
                //這是有
                if (user != null)
                {
                    // 將使用者的資訊儲存到 Session 中
                    HttpContext.Session.SetString("User", JsonConvert.SerializeObject(user));
                    
                    var IP = HttpContext.Connection.RemoteIpAddress.ToString();
                    model.login_History = new Login_History();
                    model.login_History.studentID = user.studentID;
                    model.login_History.IP_Address = IP;
                    model.login_History.login_time = DateTime.Now;
                    _context.Login_History.Add(model.login_History);
                    _context.SaveChanges();
                    
                    return RedirectToAction("Index", "Home");
                }

                //這是錯誤
                else
                {
                    ModelState.AddModelError("", "帳號或密碼錯誤");
                }
            }
            return View(model);
        }
        
        public IActionResult Sign_out()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login","Member");
        }


        public IActionResult MyInformation()
        {
            var userJson = HttpContext.Session.GetString("User");
            if(userJson != null){
                var user = JsonConvert.DeserializeObject<Member>(userJson);
                var MemberId = user.id;
                var MemberID = _context.member.Find(MemberId);
                return View(MemberID);
            }
            else{
                return RedirectToAction("Login");
            }

        }

        [HttpPost]
        public async Task<IActionResult> UploadPhoto(IFormFile photo, Member model)
        {
            try
            {
                var userJson = HttpContext.Session.GetString("User");
                var user = JsonConvert.DeserializeObject<Member>(userJson);
                var MemberId = user.id;

                if (photo != null && photo.Length > 0) 
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png"};
                    var fileExtension = Path.GetExtension(photo.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { message = "只允許上傳圖片，JPG/JPEG/PNG檔案照片！" });
                    }

                    var fileName = $"{user.studentID}_{user.student_name}{Path.GetExtension(photo.FileName)}";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/PersonalIMG", fileName);
            
                    var MemberID = _context.member.Find(MemberId);
                    if (MemberID == null)
                    {
                        return NotFound();
                    }

                    var oldFileName = MemberID.myPhoto;
                    if (!string.IsNullOrEmpty(oldFileName))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/PersonalIMG", oldFileName);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await photo.CopyToAsync(stream);
                    }

                    MemberID.myPhoto = fileName;
                    await _context.SaveChangesAsync();
                    return Json(new { photoUrl = "/PersonalIMG/" + fileName });
                }

                return BadRequest(new { message = "未上傳頭貼~" });
            }
            catch (Exception ex)
            {
                //傳送錯誤訊息至前端
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> RemovePhoto(Member model)
        {
            var userJson = HttpContext.Session.GetString("User");
            var user = JsonConvert.DeserializeObject<Member>(userJson);
            var MemberId = user.id;

            var MemberID = _context.member.Find(MemberId);
            if (MemberID == null)
            {
                return NotFound();
            }

            //刪除實體檔案
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/PersonalIMG", MemberID.myPhoto);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            MemberID.myPhoto = null;
            await _context.SaveChangesAsync();

            return Json(new { message = "Photo removed" });
        }
        
        //抓取頭貼
        public ActionResult GetPhoto()
        {
            var userJson = HttpContext.Session.GetString("User");
            if (!string.IsNullOrEmpty(userJson))
            {
                var user = JsonConvert.DeserializeObject<Member>(userJson);
                var MemberID = user.id;

                var Member = _context.member.Find(MemberID);
                if (Member != null && !string.IsNullOrEmpty(Member.myPhoto))
                {
                    var photoUrl = "/PersonalIMG/" + Member.myPhoto;
                    return Json(new { photoUrl = photoUrl });
                }
            }
            return Json(new { photoUrl = "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png" });
        }

        [HttpPost]
        public ActionResult CheckNotify([FromBody]Member model)
        {
            var userJson = HttpContext.Session.GetString("User");
            var user = JsonConvert.DeserializeObject<Member>(userJson);
            var MemberID = user.id;
            var Member = _context.member.FirstOrDefault(u => u.id == MemberID);
            Member.CheckNotify = model.CheckNotify;
            System.Console.WriteLine($"model.CheckNotify：{model.CheckNotify}");
            _context.SaveChanges();
            return Json(new { success = true, message = "Notification setting updated." });
        }

        //我的貼文-尋找公司
        public async Task<IActionResult> Mytrend_FI()
        {           
            var userJson = HttpContext.Session.GetString("User");
            var user = JsonConvert.DeserializeObject<Member>(userJson);
            var MemberID = _context.member.Find(user.id);
            var myPhoto = string.IsNullOrEmpty(MemberID.myPhoto?.ToString()) ? "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png" : "/PersonalIMG/" + MemberID.myPhoto;
            var viewModel = new MyTrendViewModel
            {
                MyFindInterns = _context.find_intern.ToList(),
                MyPhoto = myPhoto,
                FilesPost = _context.files_Post.ToList()
            };
            return View(viewModel);
        }     

        //我的貼文-就業經驗
        public async Task<IActionResult> Mytrend_SE()
        {          
            var userJson = HttpContext.Session.GetString("User");
            var user = JsonConvert.DeserializeObject<Member>(userJson);
            var MemberID = _context.member.Find(user.id);
            var myPhoto = string.IsNullOrEmpty(MemberID.myPhoto?.ToString()) ? "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png" : "/PersonalIMG/" + MemberID.myPhoto;
            var viewModel = new MyTrendViewModel
            {
                MyShareEmps = _context.share_emp.Where(i => i.studentID == MemberID.studentID && i.name == MemberID.student_name && i.dept == MemberID.department).ToList(),               
                MyPhoto = myPhoto
            };
            return View(viewModel);
        }

        //我的貼文-實習經驗
        public async Task<IActionResult> Mytrend_SI()
        {
            var userJson = HttpContext.Session.GetString("User");
            var user = JsonConvert.DeserializeObject<Member>(userJson);
            var MemberID = _context.member.Find(user.id);
            var myPhoto = string.IsNullOrEmpty(MemberID.myPhoto?.ToString()) ? "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png" : "/PersonalIMG/" + MemberID.myPhoto;
            var viewModel = new MyTrendViewModel
            {
                MyShareInterns = _context.share_intern.Where(i => i.studentID == MemberID.studentID && i.name == MemberID.student_name && i.dept == MemberID.department).ToList(),                
                MyPhoto = myPhoto,
                FilesPost = _context.files_Post.ToList()
            };
            return View(viewModel);
        }

        //珍藏-尋找公司
        public async Task<IActionResult> Myfavorite_FI()
        {           
            var userJson = HttpContext.Session.GetString("User");
            var user = JsonConvert.DeserializeObject<Member>(userJson);
            var MemberID = _context.member.Find(user.id);
            var collection = await _context.collection.Where(f => f.student_Id == MemberID.studentID).ToListAsync();
            var myPhoto = string.IsNullOrEmpty(MemberID.myPhoto?.ToString()) ? "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png" : "/PersonalIMG/" + MemberID.myPhoto;
            var viewModel = new MyTrendViewModel
            {
                collection_FindInterns = _context.find_intern.ToList().Where(i => collection.Any(l => l.post_id == i.id.ToString() && l.post_Type == "公司版")).ToList(),
                MyPhoto = myPhoto,
                collections = collection,
                FilesPost = _context.files_Post.ToList()
            };
            return View(viewModel);
        }

        //珍藏-就業經驗
        public async Task<IActionResult> Myfavorite_SE()
        {           
            var userJson = HttpContext.Session.GetString("User");
            var user = JsonConvert.DeserializeObject<Member>(userJson);
            var MemberID = _context.member.Find(user.id);
            var collection = await _context.collection.Where(f => f.student_Id == MemberID.studentID).ToListAsync();
            var members = await _context.member.ToListAsync();
            var myPhoto = string.IsNullOrEmpty(MemberID.myPhoto?.ToString()) ? "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png" : "/PersonalIMG/" + MemberID.myPhoto;
            var viewModel = new MyTrendViewModel
            {
                //收藏紀錄
                collection_ShareEmps = _context.share_emp.ToList().Where(i => collection.Any(l => l.post_id == i.id.ToString() && l.post_Type == "就業版")).ToList(),
                MyPhoto = myPhoto,
                collections = collection ,
                members = members 
            };
            return View(viewModel);
        }

        //珍藏-實習經驗
        public async Task<IActionResult> Myfavorite_SI()
        {           
            var userJson = HttpContext.Session.GetString("User");
            var user = JsonConvert.DeserializeObject<Member>(userJson);
            var MemberID = _context.member.Find(user.id);
            var collection = await _context.collection.Where(f => f.student_Id == MemberID.studentID).ToListAsync();
            var members = await _context.member.ToListAsync();
            var myPhoto = string.IsNullOrEmpty(MemberID.myPhoto?.ToString()) ? "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png" : "/PersonalIMG/" + MemberID.myPhoto;
            //找這三個資料對應到的thumb跟collection結果，對應的貼文
            var viewModel = new MyTrendViewModel
            {
                //收藏紀錄
                collection_ShareInterns = _context.share_intern.ToList().Where(i => collection.Any(l => l.post_id == i.id.ToString() && l.post_Type == "實習版")).ToList(),
                MyPhoto = myPhoto,
                collections = collection ,
                members = members,
                FilesPost = _context.files_Post.ToList()
            };
            return View(viewModel);
        }

        //按讚-就業實習
        public async Task<IActionResult> Mythumb_SE()
        {
            
            var userJson = HttpContext.Session.GetString("User");
            var user = JsonConvert.DeserializeObject<Member>(userJson);
            var MemberID = _context.member.Find(user.id);
            var thumb = await _context.thumb.Where(f => f.student_Id == MemberID.studentID).ToListAsync();
            var members = await _context.member.ToListAsync();
            var myPhoto = string.IsNullOrEmpty(MemberID.myPhoto?.ToString()) ? "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png" : "/PersonalIMG/" + MemberID.myPhoto;
            var viewModel = new MyTrendViewModel
            {
                thumb_ShareEmps= _context.share_emp.ToList().Where(i => thumb.Any(f => f.post_id == i.id.ToString() && f.post_Type == "就業版")).ToList(),
                MyPhoto = myPhoto,
                thumbs = thumb ,
                members = members  
            };
            return View(viewModel);
        }
        
        //按讚-尋找公司
        public async Task<IActionResult> Mythumb_FI()
        {
            
            var userJson = HttpContext.Session.GetString("User");
            var user = JsonConvert.DeserializeObject<Member>(userJson);
            var MemberID = _context.member.Find(user.id);
            var thumb = await _context.thumb.Where(f => f.student_Id == MemberID.studentID).ToListAsync();
            var myPhoto = string.IsNullOrEmpty(MemberID.myPhoto?.ToString()) ? "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png" : "/PersonalIMG/" + MemberID.myPhoto;

            var viewModel = new MyTrendViewModel
            {
                //按讚紀錄
                thumb_FindInterns = _context.find_intern.ToList().Where(i => thumb.Any(f => f.post_id == i.id.ToString() && f.post_Type == "公司版")).ToList(),
                MyPhoto = myPhoto,
                thumbs = thumb,
                FilesPost = _context.files_Post.ToList()
            };
            return View(viewModel);
        }

        //按讚-實習經驗
        public async Task<IActionResult> Mythumb_SI()
        {
            
            var userJson = HttpContext.Session.GetString("User");
            var user = JsonConvert.DeserializeObject<Member>(userJson);
            var MemberID = _context.member.Find(user.id);
            var thumb = await _context.thumb.Where(f => f.student_Id == MemberID.studentID).ToListAsync();
            var members = await _context.member.ToListAsync();
            var myPhoto = string.IsNullOrEmpty(MemberID.myPhoto?.ToString()) ? "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png" : "/PersonalIMG/" + MemberID.myPhoto;
            var viewModel = new MyTrendViewModel
            {
                //按讚紀錄
                thumb_ShareInterns = _context.share_intern.ToList().Where(i => thumb.Any(f => f.post_id == i.id.ToString() && f.post_Type == "實習版")).ToList(),
                MyPhoto = myPhoto,
                thumbs = thumb ,
                members = members,
                FilesPost = _context.files_Post.ToList()
            };
            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }   
    }

}