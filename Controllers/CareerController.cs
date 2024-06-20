using Microsoft.AspNetCore.Mvc;
using Student_web.Data;
using Microsoft.EntityFrameworkCore;
using Student_web.Models;
using Newtonsoft.Json;
using Student_web.Services;
using Microsoft.AspNetCore.Authorization;

namespace Student_web.Controllers
{   
    [Filter.LoginFilter]
    public class CareerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILineNotifyService _lineNotifyService;

        public CareerController(ApplicationDbContext context,ILineNotifyService lineNotifyService)
        {
            _context = context;
            _lineNotifyService = lineNotifyService;
        }

        //圖表
        public IActionResult CareerEchart()
        {   
            List<int> year = _context.Career.Select(p => p.year).Distinct().ToList();
            int? maxYear = year.DefaultIfEmpty().Max();//資料年份最大
            int? minYear = year.DefaultIfEmpty().Min();//資料年份最小    
            ViewBag.MaxYear = maxYear;
            ViewBag.MinYear = minYear;
            return View();
        }

        //接收篩選
        public IActionResult SelectMap([FromForm] int selectYear,[FromForm] string selectScience,[FromForm] string selectDepartment)
        {
            System.Diagnostics.Debug.WriteLine($"Received form data: selectDepartment={selectDepartment} selectYear={selectYear} selectScience={selectScience}");
            var filteredData = _context.Career.AsQueryable();
            if (selectYear != 0)//如果 selectYear 有值，執行
            {
                filteredData = filteredData.Where(p => p.year == selectYear);
            }

            if (!string.IsNullOrEmpty(selectScience))//如果 selectScience 不是 null，有數值，執行
            {
                filteredData = filteredData.Where(p => p.science == selectScience);
            }

            if (!string.IsNullOrEmpty(selectDepartment))//如果 selectDepartment 不是 null，有數值，執行
            {
                filteredData = filteredData.Where(p => p.department == selectDepartment);
            }
            //資料總筆數
            var allCount = filteredData.Count();

            //所有公司前三名
            var top3Company = filteredData
                .GroupBy(c => c.company)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(3)
                .ToList();

            //分部地區
            var locationData = new[] {
                new { Location = "北部地區", Count =  filteredData.Count(p => p.location == "台北市" || p.location == "新北市" || p.location == "基隆市" || p.location == "新竹市" || p.location == "新竹縣" || p.location == "宜蘭縣" | p.location == "桃園市" ) },
                new { Location = "中部地區", Count =  filteredData.Count(p => p.location == "苗栗縣" || p.location == "台中市" || p.location == "彰化縣" || p.location == "南投縣" || p.location == "雲林縣") },
                new { Location = "南部地區", Count =  filteredData.Count(p => p.location == "嘉義縣" || p.location == "嘉義市" || p.location == "台南市" || p.location == "高雄市" || p.location == "屏東縣" || p.location == "澎湖縣") },
                new { Location = "東部地區", Count =  filteredData.Count(p => p.location == "花蓮縣" || p.location == "臺東縣" ) },
                new { Location = "外島地區", Count =  filteredData.Count(p => p.location == "金門縣" || p.location == "連江縣" ) },
                new { Location = "國外", Count =  filteredData.Count(p => p.location == "國外") },};

            //狀態分布
            var stateData = new[]{
                new { State = "實習", Count = filteredData.Count(p => p.state == 1 )},
                new { State = "兼職", Count = filteredData.Count(p => p.state == 2 )},
                new { State = "全職", Count = filteredData.Count(p => p.state == 3 )},};

            var positionData = filteredData
                .GroupBy(c => c.position)
                .Select(g => new
                {
                    Position = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .ToList();

            var state1Data = filteredData
                .Where(c => c.state == 1)
                .GroupBy(c => c.position)
                .Select(g => new
                {
                    Position = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .ToList();

            var state2Data = filteredData
                .Where(c => c.state == 2)
                .GroupBy(c => c.position)
                .Select(g => new
                {
                    Position = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .ToList();

            var state3Data = filteredData
                .Where(c => c.state == 3)
                .GroupBy(c => c.position)
                .Select(g => new
                {
                    Position = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .ToList();

            var combinedData = positionData.Select(p => new {
                Position = p.Position,
                State1Count = state1Data.FirstOrDefault(s => s.Position == p.Position)?.Count ?? 0,
                State2Count = state2Data.FirstOrDefault(s => s.Position == p.Position)?.Count ?? 0,
                State3Count = state3Data.FirstOrDefault(s => s.Position == p.Position)?.Count ?? 0}).ToList();

            var results = new{
                PositionData = positionData,
                LocationData = locationData,
                StateData = stateData,
                Top3Company = top3Company,
                AllCount = allCount,

                CombinedData = combinedData
            };
           
            return Json(results);
        }

        [HttpGet]//產生隨機公司代碼
        public IActionResult GenerateRandomCode()
        {
            string randomCode = Guid.NewGuid().ToString();
            var code = new Randomcode
            {
                CompanyCode = randomCode,
                CreatedAt = DateTime.Now,
            };
            _context.RandomCode.Add(code);
            _context.SaveChanges();
            return Content(randomCode);
        }
        
        [HttpPost]
        public async Task<IActionResult> InputCode(string randomCode)
        {
            if (!string.IsNullOrEmpty(randomCode))
            {
                var isValidCode = await CheckRandomCode(randomCode);
                if (!isValidCode)
                {
                    TempData["ErrorCode"] = true;
                    return RedirectToAction("Login", "Member");
                }
                else{
                    var post = _context.find_intern.FirstOrDefault(p => p.CompanyCode == randomCode)?.id;
                    if(post!=null){
                        return RedirectToAction("Find_intern_Edit", new { id = post });
                    }
                    else{
                        return RedirectToAction("Find_internWeb");
                    }                  
                }   
            }
            TempData["NoCode"] = true;
            return RedirectToAction("Login", "Member");
        }

        private async Task<bool> CheckRandomCode(string code)
        {
            //檢查代碼是否有效
            var randomCode = await _context.RandomCode.FirstOrDefaultAsync(c => c.CompanyCode == code && !c.CodeState);
            if (randomCode != null)
            {
                if ((DateTime.Now - randomCode.CreatedAt).TotalDays >= 14){
                    randomCode.CodeState = true;
                    await _context.SaveChangesAsync();
                    return false;
                }
                else{
                    randomCode.CreatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("randomCode", JsonConvert.SerializeObject(randomCode));
                    return true;
                }               
            }
            return false;
        }

        private bool IsUserLoggedIn()
        {
           return HttpContext.Session.GetString("user") != null || HttpContext.Session.GetString("randomCode") != null;
        }
        
        //公司實習的表單
        [AllowAnonymous]
        public IActionResult Find_internWeb()
        {   
            bool isUserLoggedIn = HttpContext.Session.GetString("User") != null;
            bool isCodeEntered = HttpContext.Session.GetString("randomCode") != null;
            if(isUserLoggedIn||isCodeEntered){
                return View();
            }

            return RedirectToAction("Login", "Member");            
        }
        
        [HttpPost]
        public async Task<IActionResult> Find_internWeb(List<IFormFile> files, Find_intern model)
        {
            ModelState.Remove("file");
            ModelState.Remove("randomCode");
            
            if (ModelState.IsValid)
            {
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };//限定傳檔案類型
                var fileNames = new List<string>();
                var fileTypes = new List<string>();
                var fileStreams = new List<MemoryStream>();
                var fileSizes = new List<long>();//存檔案size

                if (files != null)
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            //檔案驗證
                            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                            if (!allowedExtensions.Contains(fileExtension))
                            {
                                ModelState.AddModelError(string.Empty, "只能上傳PDF跟圖檔");
                                ViewBag.ErrorMessage = "只能上傳PDF跟圖檔";
                                return View(model);
                            }

                            if (file.Length > 5 * 1024 * 1024)
                            {
                                ModelState.AddModelError(string.Empty, "文件大小不可超過5MB");
                                ViewBag.ErrorMessage = "文件大小不可超過5MB";
                                return View(model);
                            }

                            var memoryStream = new MemoryStream();
                            await file.CopyToAsync(memoryStream);
                            fileStreams.Add(memoryStream);

                            var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");//動態命名
                            var newFileName = $"公司版_{timeStamp}_{file.FileName}";
                            //存取檔案資料
                            fileNames.Add(newFileName);
                            fileTypes.Add(file.ContentType);
                            fileSizes.Add(file.Length);
                        }
                    }
                }
                model.create_time = DateTime.Now;
                var randomCode = HttpContext.Session.GetString("randomCode");
                if(randomCode!=null){
                    var code = JsonConvert.DeserializeObject<Randomcode>(randomCode);
                    var CompanyCode = code.CompanyCode;
                    model.CompanyCode = CompanyCode;
                }
                _context.find_intern.Add(model);//先存檔在抓取id
                _context.SaveChanges();

                int postId = model.id;//抓取剛存好的貼文id
                for (int i = 0; i < fileNames.Count; i++)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PostFile", fileNames[i]);
                    System.IO.File.WriteAllBytes(filePath, fileStreams[i].ToArray());//存到指定的本地端位置

                    var filePost = new Files_post
                    {
                        file_name = fileNames[i],
                        file_type = fileTypes[i],
                        post_id = postId,
                        post_Type = "公司版",
                        file_size = fileSizes[i],
                        upload_time = DateTime.Now
                    };
                    _context.files_Post.Add(filePost);
                }

                _context.SaveChanges();
                var users = _context.member.ToList(); // Example: fetching all users
                foreach (var user in users)
                {
                    if(user.CheckNotify){
                        _lineNotifyService.SendNotify(user.AccessToken, $"尋找公司版上新增了《{model.item}》實習招募，快去看吧~", 8525, 16581305);
                    }
                }
                TempData["MarqueeMessage"] = $"{model.company_name} 新增了實習機會：【{model.item}】！！！";
                TempData["SuccessfulPost"] = true;
                if (HttpContext.Session.GetString("User") == null)
                {   
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login","Member");
                }
                else
                {
                    return RedirectToAction("Post_Version");
                }
            }
            return View(model);//驗證失敗，傳回錯誤值 
        }


        //就業分享的表單
        public IActionResult Share_empWeb()
        {
            return View();
        }
        
        [HttpPost]
        public ActionResult Share_empWeb(Share_emp_ViewModel model)
        {   
            //移除預設驗證
            ModelState.Remove("share_emps.studentID");
            ModelState.Remove("share_emps.name");
            ModelState.Remove("share_emps.dept");
            var user = HttpContext.Session.GetString("User");//session 抓到現在的使用者
            if (string.IsNullOrEmpty(user))
            {
                return RedirectToAction("Login", "Member");
            }
            if (ModelState.IsValid)
            {
                var data = JsonConvert.DeserializeObject<Member>(user);//反序列抓到的使用者，將資料抓出來
                var MemberId = data.id;//設定變數
                var MemberID = _context.member.Find(MemberId);//去撈出id對應的member的資料

                //存取在前端所填寫的資料
                model.share_emps.studentID = MemberID.studentID;
                model.share_emps.name = MemberID.student_name;
                model.share_emps.dept = MemberID.department;
                model.share_emps.create_time = DateTime.Now;
                
                _context.share_emp.Add(model.share_emps);
                _context.SaveChanges();
                _lineNotifyService.SendNotify(MemberID.AccessToken, $"您成功新增了《{model.share_emps.item}》在就業分享版上",8525,16581290);//LineNotify通知
                TempData["MarqueeMessage"] = $"{model.share_emps.name} 新增了：《{model.share_emps.item}》在就業分享版上~";
                TempData["SuccessfulPost"] = true;
                return RedirectToAction("Post_Version1");
            }
            return View(model);
        }

        //實習經驗的表單
        public IActionResult Share_internWeb()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Share_internWeb(List<IFormFile> files, Share_intern_ViewModel model,string SwitchValue)
        {   
            ModelState.Remove("file");//移除檔案的驗證
            ModelState.Remove("SwitchValue");//移除切換開端的驗證
            var user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user))
            {
                return RedirectToAction("Login", "Member");
            }
            if (ModelState.IsValid)
            {
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };//設定檔案類型
                var fileNames = new List<string>();
                var fileTypes = new List<string>();
                var fileStreams = new List<MemoryStream>();
                var fileSizes = new List<long>();
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            //驗證檔案類型是否為PDF跟圖片
                            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                            if (!allowedExtensions.Contains(fileExtension))
                            {
                                ModelState.AddModelError(string.Empty, "只能上傳PDF跟圖檔");
                                ViewBag.ErrorMessage = "只能上傳PDF跟圖檔";
                                return View(model);
                            }
                            //檔案不可超過5MB
                            if (file.Length > 5 * 1024 * 1024)
                            {
                                ModelState.AddModelError(string.Empty, "文件大小不可超過5MB");
                                ViewBag.ErrorMessage = "文件大小不可超過5MB";
                                return View(model);
                            }

                            var memoryStream = new MemoryStream();//轉二進位
                            await file.CopyToAsync(memoryStream);
                            fileStreams.Add(memoryStream);

                            var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            var newFileName = $"實習版_{timeStamp}_{file.FileName}";//動態命名
                            fileNames.Add(newFileName);
                            fileTypes.Add(file.ContentType);
                            fileSizes.Add(file.Length);
                        }
                    }
                }
                //反序列session的資料
                var data = JsonConvert.DeserializeObject<Member>(user);
                var MemberID = _context.member.Find(data.id);

                //存取session的user的資料
                model.share_interns.studentID = MemberID.studentID;
                model.share_interns.name = MemberID.student_name;
                model.share_interns.dept = MemberID.department;
                model.share_interns.create_time = DateTime.Now;
                model.share_interns.anonymous = SwitchValue;
                _context.share_intern.Add(model.share_interns);
                _context.SaveChanges();

                int postId = model.share_interns.id;//剛存好的id
                for (int i = 0; i < fileNames.Count; i++)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PostFile", fileNames[i]);
                    System.IO.File.WriteAllBytes(filePath, fileStreams[i].ToArray());//存到本地端

                    var filePost = new Files_post
                    {
                        file_name = fileNames[i],
                        file_type = fileTypes[i],
                        post_id = postId,
                        post_Type = "實習版",
                        file_size = fileSizes[i],
                        upload_time = DateTime.Now
                    };
                    _context.files_Post.Add(filePost);
                }
                _context.SaveChanges();
                _lineNotifyService.SendNotify(MemberID.AccessToken, $"您成功新增了《{model.share_interns.item}》在實習經驗版上",8515,16581243);//LineNotify通知
                TempData["MarqueeMessage"] = $"{model.share_interns.name} 新增了：《{model.share_interns.item}》在實習經驗版上~";
                TempData["SuccessfulPost"] = true;
                return RedirectToAction("Post_Version2");
            }
            return View(model);
        }    




        //尋找公司的貼文區
        public async Task<IActionResult> Post_Version(string searchKeyword, int idToDelete)
        {
            var result = _context.find_intern.AsQueryable();
            var user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user))
            {
                return RedirectToAction("Login", "Member");
            }
            var data = JsonConvert.DeserializeObject<Member>(user);
            var MemberID = _context.member.Find(data.id);
            //搜尋
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                foreach (var word in searchKeyword.ToCharArray())
                {
                    result = result.Where(x => x.company_name.Contains(word.ToString())|| x.item.Contains(word.ToString())|| x.work_Content.Contains(word.ToString()));
                }
            }            
            //刪除
            if (idToDelete != 0)
            {
                var itemToDelete = await _context.find_intern.FindAsync(idToDelete);
                if (itemToDelete != null)
                {
                    //抓貼文對應id跟版面類型的資料，thumb、messege_post、collection
                    var filesToDelete = await _context.files_Post.Where(m => m.post_id == idToDelete && m.post_Type == "公司版").ToListAsync();
                    var messageToDelete = await _context.Message_post.Where(m => m.post_id == idToDelete && m.post_Type == "公司版").ToListAsync();
                    var thumbToDelete = await _context.thumb.Where(m => m.post_id == idToDelete.ToString() && m.post_Type == "公司版").ToListAsync();
                    var keepToDelete = await _context.collection.Where(m => m.post_id == idToDelete.ToString() && m.post_Type == "公司版").ToListAsync();

                    foreach (var file in filesToDelete)
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PostFile", file.file_name);//刪除本地端檔案
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    _context.files_Post.RemoveRange(filesToDelete);
                    _context.Message_post.RemoveRange(messageToDelete);
                    _context.thumb.RemoveRange(thumbToDelete);
                    _context.collection.RemoveRange(keepToDelete);
                    _context.find_intern.Remove(itemToDelete);
                    await _context.SaveChangesAsync();//更新所有資料表
                }
            }

            var shares = await result.ToListAsync();
            var shareViewModels = new List<MessageViewModel>();
            var messageCounts = new Dictionary<int, int>();
            foreach (var share in shares)
            {   
                //處理files_Post、Message_post、thumb、collection的資料表資料，顯示在前端
                var files = await _context.files_Post.Where(f => f.post_id == share.id && f.post_Type == "公司版").ToListAsync();
                var messageCount = await _context.Message_post.CountAsync(m => m.post_id == share.id && m.post_Type == "公司版");
                var likesCount = await _context.thumb.CountAsync(m => m.post_id == share.id.ToString() && m.post_Type == "公司版");
                var userHasLiked = await _context.thumb.AnyAsync(m => m.student_Id == MemberID.studentID && m.post_id == share.id.ToString() && m.post_Type == "公司版");
                var userHaskeeped = await _context.collection.AnyAsync(m => m.student_Id == MemberID.studentID && m.post_id == share.id.ToString() && m.post_Type == "公司版");
                messageCounts.Add(share.id, messageCount);
                var viewModel = new MessageViewModel
                {
                    find_interns = share,
                    member = new Member(),
                    message_Post = new Message_post(),
                    thumbs = new Thumb(),
                    UserHasKeeped = userHaskeeped,
                    UserHasLiked = userHasLiked ,
                    Files_posts = files,
                };

                shareViewModels.Add(viewModel);
            }
            ViewBag.MessageCounts = messageCounts;//留言數
            return View(shareViewModels);
        }

        //就業分享的貼文區
        public async Task<IActionResult> Post_Version1(string searchKeyword, int idToDelete)
        {
            var result = _context.share_emp.AsQueryable();
            var user = HttpContext.Session.GetString("User");//session的User
            if (string.IsNullOrEmpty(user))
            {
                return RedirectToAction("Login", "Member");
            }
            var data = JsonConvert.DeserializeObject<Member>(user);//反序列
            var MemberID = _context.member.Find(data.id);//找到member對應資料
 
            //搜尋
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                foreach (var word in searchKeyword.ToCharArray())
                {
                    result = result.Where(x => x.company.Contains(word.ToString())|| x.item.Contains(word.ToString())|| x.content.Contains(word.ToString()));
                }
            }
            //刪除貼文，有關的資料表資料
            if (idToDelete != 0)
            {
                var itemToDelete = await _context.share_emp.FindAsync(idToDelete);
                if (itemToDelete != null)
                {
                    _context.share_emp.Remove(itemToDelete);
                    var messageToDelete = await _context.Message_post.FirstOrDefaultAsync(m => m.id == idToDelete && m.post_Type == "就業版");
                    var thumbToDelete = await _context.thumb.Where(m => m.post_id == idToDelete.ToString() && m.post_Type == "就業版").ToListAsync();
                    var keepToDelete = await _context.collection.Where(m => m.post_id == idToDelete.ToString() && m.post_Type == "就業版").ToListAsync();
                    if (messageToDelete != null){
                        _context.Message_post.RemoveRange(messageToDelete);
                    }
                    if(thumbToDelete !=null){
                        _context.thumb.RemoveRange(thumbToDelete);
                    }
                    if(keepToDelete !=null){
                        _context.collection.RemoveRange(keepToDelete);
                    }
                    await _context.SaveChangesAsync();//更新所有的資料
                }
            }
            var shares = await result.ToListAsync();
            var shareViewModels = new List<Share_emp_ViewModel>();
            var messageCounts = new Dictionary<int, int>();
            foreach (var share in shares)
            {
                var member = await _context.member.Where(m => m.studentID == share.studentID).FirstOrDefaultAsync();//使用student_id抓member的對應學號資料
                var messageCount = await _context.Message_post.CountAsync(m => m.post_id == share.id && m.post_Type == "就業版");
                var likesCount = await _context.thumb.CountAsync(m => m.post_id == share.id.ToString() && m.post_Type == "就業版");
                var userHasLiked = await _context.thumb.AnyAsync(m => m.student_Id == MemberID.studentID && m.post_id == share.id.ToString() && m.post_Type == "就業版");
                var userHaskeeped = await _context.collection.AnyAsync(m => m.student_Id == MemberID.studentID && m.post_id == share.id.ToString() && m.post_Type == "就業版");
                messageCounts.Add(share.id, messageCount);
                if (member != null)
                {
                    var photoPath = member.myPhoto != null ? Path.Combine("/PersonalIMG/", member.myPhoto) 
                        : "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png";
                    
                    //新增物件，宣告Share_emp_ViewModel
                    var viewModel = new Share_emp_ViewModel
                    {
                        member = member,
                        share_emps = share,
                        photoPath = photoPath,
                        message_Post = new Message_post(),
                        thumbs = new Thumb(),
                        UserHasKeeped = userHaskeeped,
                        UserHasLiked = userHasLiked 
                    };

                    shareViewModels.Add(viewModel);
                }
            }
            ViewBag.MessageCounts = messageCounts;
            return View(shareViewModels);
        }

        //實習經驗的貼文區
        public async Task<IActionResult> Post_Version2(string searchKeyword, int idToDelete)
        {
            var result = _context.share_intern.AsQueryable();
            var user = HttpContext.Session.GetString("User");//抓目前使用者的session
            if (string.IsNullOrEmpty(user))
            {
                return RedirectToAction("Login", "Member");
            }
            var data = JsonConvert.DeserializeObject<Member>(user);//反序列
            var MemberID = _context.member.Find(data.id);

            //搜尋
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                 foreach (var word in searchKeyword.ToCharArray())
                {
                    result = result.Where(x => x.company_name.Contains(word.ToString())|| x.item.Contains(word.ToString())|| x.work_content.Contains(word.ToString()));
                }
            }

            if (idToDelete != 0)
            {
                var itemToDelete = await _context.share_intern.FindAsync(idToDelete);
                if (itemToDelete != null)
                {
                    //抓貼文對應id跟版面類型的資料，thumb、messege_post、collection
                    var filesToDelete = await _context.files_Post.Where(m => m.post_id == idToDelete && m.post_Type == "實習版").ToListAsync();
                    var messageToDelete = await _context.Message_post.Where(m => m.post_id == idToDelete && m.post_Type == "實習版").ToListAsync();
                    var thumbToDelete = await _context.thumb.Where(m => m.post_id == idToDelete.ToString() && m.post_Type == "實習版").ToListAsync();
                    var keepToDelete = await _context.collection.Where(m => m.post_id == idToDelete.ToString() && m.post_Type == "實習版").ToListAsync();

                    foreach (var file in filesToDelete)
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PostFile", file.file_name);//刪除本地端檔案
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    _context.files_Post.RemoveRange(filesToDelete);
                    _context.Message_post.RemoveRange(messageToDelete);
                    _context.thumb.RemoveRange(thumbToDelete);
                    _context.collection.RemoveRange(keepToDelete);
                    _context.share_intern.Remove(itemToDelete);
                    await _context.SaveChangesAsync();//更新所有資料表
                }
            }
            var shares = await result.ToListAsync();
            var shareViewModels = new List<Share_intern_ViewModel>();
            var messageCounts = new Dictionary<int, int>();
            foreach (var share in shares)
            {   
                //前端放置有關實習版的其他資料表
                var files = await _context.files_Post.Where(f => f.post_id == share.id && f.post_Type == "實習版").ToListAsync();
                var member = await _context.member.Where(m => m.studentID == share.studentID).FirstOrDefaultAsync();
                var messageCount = await _context.Message_post.CountAsync(m => m.post_id == share.id && m.post_Type == "實習版");
                var likesCount = await _context.thumb.CountAsync(m => m.post_id == share.id.ToString() && m.post_Type == "實習版");
                var userHasLiked = await _context.thumb.AnyAsync(m => m.student_Id == MemberID.studentID && m.post_id == share.id.ToString() && m.post_Type == "實習版");
                var userHaskeeped = await _context.collection.AnyAsync(m => m.student_Id == MemberID.studentID && m.post_id == share.id.ToString() && m.post_Type == "實習版");
                messageCounts.Add(share.id, messageCount);
                if (member != null)
                {
                    var photoPath = member.myPhoto != null ? Path.Combine("/PersonalIMG/", member.myPhoto) 
                        : "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png";

                    //新增物件，宣告Share_emp_ViewModel
                    var viewModel = new Share_intern_ViewModel
                    {
                        member = member,
                        share_interns = share,
                        photoPath = photoPath,
                        message_Post = new Message_post(),
                        thumbs = new Thumb(),
                        UserHasKeeped = userHaskeeped,
                        UserHasLiked = userHasLiked,
                        Files_posts = files,
                    };
                    shareViewModels.Add(viewModel);
                }
            }
            ViewBag.MessageCounts = messageCounts;
            return View(shareViewModels);
        }
       
       
        //彈跳視窗
        public ActionResult Action(int id)
        {   
            var find_intern = _context.find_intern.Find(id);
            var messagePosts = _context.Message_post.Where(m => m.post_id == id).ToList();
            var messageCounts = _context.Message_post.Count(m => m.post_id == id && m.post_Type == "公司版");
            var files = _context.files_Post.Where(f => f.post_id == id && f.post_Type == "公司版").ToList();
            var user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user))
            {
                return RedirectToAction("Login", "Member");
            }
            var data = JsonConvert.DeserializeObject<Member>(user);
            var MemberID = _context.member.Find(data.id);
            var viewModel = new MessageViewModel
            {
                find_interns = find_intern,
                member = new Member(),
                message_Post = new Message_post(),
                MemberPhoto = new Dictionary<string, string>(),//儲存學生的頭貼
                thumbs = new Thumb(),
                Files_posts = files,
                Myphoto = MemberID.myPhoto != null ? "/PersonalIMG/" + MemberID.myPhoto : "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png"
            };           
            foreach(var message in messagePosts)
            {
                var member = _context.member.FirstOrDefault(m => m.student_name == message.student_Name);
                if (member != null)
                {
                    viewModel.MemberPhoto[message.student_Name] = member.myPhoto != null ? "/PersonalIMG/" + member.myPhoto : "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png";
                }
            }
            ViewBag.MessageCounts = messageCounts;
            var tupleModel = new Tuple<MessageViewModel, List<Message_post>>(viewModel, messagePosts);
            return PartialView("DataView", tupleModel);
        }

        public ActionResult Action1(int id)
        {   
            var share_emp = _context.share_emp.Find(id);
            var Post_member = _context.member.FirstOrDefault(m => m.studentID == share_emp.studentID && m.student_name == share_emp.name && m.department == share_emp.dept);
            var messagePosts = _context.Message_post.Where(m => m.post_id == id && m.post_Type == "就業版").ToList();
            var messageCounts = _context.Message_post.Count(m => m.post_id == id && m.post_Type == "就業版");
            var user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user))
            {
                return RedirectToAction("Login", "Member");
            }
            var data = JsonConvert.DeserializeObject<Member>(user);
            var MemberID = _context.member.Find(data.id);
            var viewModel = new Share_emp_ViewModel
            {
                share_emps = share_emp,
                userPhoto = Post_member.myPhoto != null ? "/PersonalIMG/" + Post_member.myPhoto : "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png",
                message_Post = new Message_post(),
                MemberPhoto = new Dictionary<string, string>(),//儲存學生的頭貼
                thumbs = new Thumb(),
                Myphoto = MemberID.myPhoto != null ? "/PersonalIMG/" + MemberID.myPhoto : "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png"
            };
            foreach(var message in messagePosts)
            {
                var member = _context.member.FirstOrDefault(m => m.student_name == message.student_Name);
                if (member != null)
                {
                    viewModel.MemberPhoto[message.student_Name] = member.myPhoto != null ? "/PersonalIMG/" + member.myPhoto : "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png";
                }
            }
            ViewBag.MessageCounts = messageCounts;
            var tupleModel = new Tuple<Share_emp_ViewModel, List<Message_post>>(viewModel, messagePosts);
            return PartialView("DataView1", tupleModel);
        }

        public ActionResult Action2(int id)
        {   
            var share_intern = _context.share_intern.Find(id);
            var Post_member = _context.member.FirstOrDefault(m => m.studentID == share_intern.studentID && m.student_name == share_intern.name && m.department == share_intern.dept);
            var messagePosts = _context.Message_post.Where(m => m.post_id == id && m.post_Type == "實習版").ToList();
            var messageCounts = _context.Message_post.Count(m => m.post_id == id && m.post_Type == "實習版");
            var files = _context.files_Post.Where(f => f.post_id == id && f.post_Type == "實習版").ToList();
            var user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user))
            {
                return RedirectToAction("Login", "Member");
            }
            var data = JsonConvert.DeserializeObject<Member>(user);
            var MemberID = _context.member.Find(data.id);
            var viewModel = new Share_intern_ViewModel
            {
                share_interns = share_intern,
                userPhoto = Post_member.myPhoto != null ? "/PersonalIMG/" + Post_member.myPhoto : "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png",
                message_Post = new Message_post(),
                MemberPhoto = new Dictionary<string, string>(),//儲存學生的頭貼
                thumbs = new Thumb(),
                Files_posts = files,
                Myphoto = MemberID.myPhoto != null ? "/PersonalIMG/" + MemberID.myPhoto : "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png"
            };
            foreach(var message in messagePosts)
            {
                var member = _context.member.FirstOrDefault(m => m.student_name == message.student_Name);
                if (member != null)
                {
                    viewModel.MemberPhoto[message.student_Name] = member.myPhoto != null ? "/PersonalIMG/" + member.myPhoto : "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png";
                }
            }
            ViewBag.MessageCounts = messageCounts;
            var tupleModel = new Tuple<Share_intern_ViewModel, List<Message_post>>(viewModel, messagePosts);
            return PartialView("DataView2", tupleModel);
        }

        //留言
        [HttpPost]
        public JsonResult find_intern_Message(int id,MessageViewModel UserMessage)
        {
            if (ModelState.IsValid)
            {
                var user = HttpContext.Session.GetString("User");
                var data = JsonConvert.DeserializeObject<Member>(user);
                var MemberID = _context.member.Find(data.id);
                var Author = _context.member.FirstOrDefault(p => p.Level == "管理員");
                var post = _context.find_intern.Find(id);
                string Name = MemberID.student_name.Length < 3 ? MemberID.student_name.Substring(0, 1) 
                                                                : MemberID.student_name[0] + new string('○', MemberID.student_name.Length - 2) + MemberID.student_name[^1];//名字中間變○，ex.王○明
                UserMessage.message_Post = new Message_post();
                if (user != null)
                {                     
                    //將Session中的學生ID的對應資料存到UserMessage.message中
                    UserMessage.message_Post.student_Id = MemberID.studentID;
                    UserMessage.message_Post.student_Name = MemberID.student_name;
                    UserMessage.userPhoto = MemberID.myPhoto != null ? "/PersonalIMG/" + MemberID.myPhoto : "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png";
                }
                UserMessage.message_Post.message = Request.Form["message"];
                UserMessage.message_Post.post_id = id;              
                UserMessage.message_Post.post_Type = Request.Form["post_type"];
                UserMessage.message_Post.message_time = DateTime.Now;

                _context.Add(UserMessage.message_Post);
                _context.SaveChanges();
                _lineNotifyService.SendNotify(Author.AccessToken, $"{Name}在您的貼文《{post.item}》裡留言",null,null);
                var messageCount = _context.Message_post.CountAsync(m => m.post_id == id && m.post_Type == Request.Form["post_type"]);
                return Json(UserMessage);
            }
            return Json("Post_Version", UserMessage);
        }

        [HttpPost]
        public JsonResult share_emp_Message(int id,Share_emp_ViewModel UserMessage)
        {
            if (ModelState.IsValid)
            {
                var user = HttpContext.Session.GetString("User");
                var data = JsonConvert.DeserializeObject<Member>(user);
                var MemberID = _context.member.Find(data.id);
                var post = _context.share_emp.Find(id);
                var Author = _context.member.FirstOrDefault(p => p.studentID == post.studentID);
                string Name = MemberID.student_name.Length < 3 ? MemberID.student_name.Substring(0, 1) 
                                                                : MemberID.student_name[0] + new string('○', MemberID.student_name.Length - 2) + MemberID.student_name[^1];//名字中間變○，ex.王○明
                
                UserMessage.message_Post = new Message_post();
                if (user != null)
                {                     
                    //將Session中的學生ID的對應資料存到UserMessage.message中
                    UserMessage.message_Post.student_Id = MemberID.studentID;
                    UserMessage.message_Post.student_Name = MemberID.student_name;
                    UserMessage.userPhoto = MemberID.myPhoto != null ? "/PersonalIMG/" + MemberID.myPhoto : "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png";
                }
                UserMessage.message_Post.message = Request.Form["message"];
                UserMessage.message_Post.post_id = id;              
                UserMessage.message_Post.post_Type = Request.Form["post_type"];
                UserMessage.message_Post.message_time = DateTime.Now;

                _context.Add(UserMessage.message_Post);
                _context.SaveChanges();
                _lineNotifyService.SendNotify(Author.AccessToken, $"{Name}在您的貼文《{post.item}》裡留言",null,null);
                return Json(UserMessage);
            }
            return Json("Post_Version1", UserMessage);
        }

        [HttpPost]
        public JsonResult  share_intern_Message(int id,Share_intern_ViewModel UserMessage)
        {
            if (ModelState.IsValid)
            {
                var user = HttpContext.Session.GetString("User");
                var data = JsonConvert.DeserializeObject<Member>(user);
                var MemberID = _context.member.Find(data.id);
                var post = _context.share_intern.Find(id);
                var Author = _context.member.FirstOrDefault(p => p.studentID == post.studentID);
                string Name = MemberID.student_name.Length < 3 ? MemberID.student_name.Substring(0, 1) //if
                                                               : MemberID.student_name[0] + new string('○', MemberID.student_name.Length - 2) + MemberID.student_name[^1];//名字中間變○，ex.王○明
                
                UserMessage.message_Post = new Message_post();
                if (user != null)
                {                      
                    //將Session中的學生ID的對應資料存到UserMessage.message中
                    UserMessage.message_Post.student_Id = MemberID.studentID;
                    UserMessage.message_Post.student_Name = MemberID.student_name;
                    UserMessage.userPhoto = MemberID.myPhoto != null ? "/PersonalIMG/" + MemberID.myPhoto : "https://imgs.gotrip.hk/wp-content/uploads/2017/11/nhv4dxh3MJN7gxp/blank-profile-picture-973460_960_720_2583405935a02dfab699c6.png";
                }
                UserMessage.message_Post.message = Request.Form["message"];
                UserMessage.message_Post.post_id = id;              
                UserMessage.message_Post.post_Type = Request.Form["post_type"];
                UserMessage.message_Post.message_time = DateTime.Now;

                _context.Add(UserMessage.message_Post);
                _context.SaveChanges();
                _lineNotifyService.SendNotify(Author.AccessToken, $"{Name}在您的貼文《{post.item}》裡留言",null,null);
                return Json(UserMessage);
            }
            return Json("Post_Version2", UserMessage);
        }

        [HttpPost]
        public ActionResult DeleteMessage(int id, string post_Type)
        {
            var message = _context.Message_post.Find(id);
            _context.Message_post.Remove(message);
            _context.SaveChanges();
            return Json(new { success = true });
        }
         //修改
        [AllowAnonymous]
        public async Task<IActionResult> Find_intern_Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var intern = await _context.find_intern.FindAsync(id);
            if (intern == null)
            {
                return NotFound();
            }

            var files = await _context.files_Post.Where(fp => fp.post_id == id && fp.post_Type == "公司版").ToListAsync();
            var viewModel = new MessageViewModel
            {
                find_interns = intern,
                Files_posts = files
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Find_intern_Edit(int? id, MessageViewModel model)
        {
            if (id != model.find_interns?.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                //抓取貼文
                var intern = await _context.find_intern.FindAsync(id);
                
                if (intern == null)
                {
                    return NotFound();
                }

                //更新所修改的資料
                intern.item = model.find_interns.item;
                intern.company_name = model.find_interns.company_name;
                intern.location = model.find_interns.location;
                intern.work_Content = model.find_interns.work_Content;
                intern.salary = model.find_interns.salary;
                intern.links = model.find_interns.links;
                _context.Update(intern);

                //找到貼文ID對應到的Files_post的檔案
                var existingFiles = await _context.files_Post.Where(fp => fp.post_id == id && fp.post_Type == "公司版").ToListAsync();

                //計算檔案大小
                long existingFilesSize = existingFiles.Sum(fp => fp.file_size);
                long newFilesSize = model.NewFiles?.Sum(f => f.Length) ?? 0;

                if (existingFilesSize + newFilesSize > 5 * 1024 * 1024) //5MB
                {
                    ModelState.AddModelError(string.Empty, "總檔案大小不能超過 5MB");
                    ViewBag.ErrorMessage = "總檔案大小不能超過 5MB";
                    model.Files_posts = existingFiles;
                    return View(model);
                }

                //刪除檔案
                if (model.FilesToDelete != null)
                {
                    var filesToDelete = existingFiles.Where(fp => model.FilesToDelete.Contains(fp.Id)).ToList();
                    foreach (var file in filesToDelete)
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PostFile", file.file_name);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                        _context.files_Post.Remove(file);
                    }
                }

                //新增檔案
                if (model.NewFiles != null)
                {
                    foreach (var file in model.NewFiles)
                    {
                        var allowedFileTypes = new[] { "application/pdf", "image/jpeg", "image/png" };
                        if (!allowedFileTypes.Contains(file.ContentType))
                        {
                            ModelState.AddModelError(string.Empty, "只能上傳PDF或圖片檔案");
                            ViewBag.ErrorMessage = "只能上傳PDF或圖片檔案";
                            model.Files_posts = existingFiles;
                            return View(model);
                        }
                        if (file.Length > 0)
                        {
                            var memoryStream = new MemoryStream();
                            await file.CopyToAsync(memoryStream);
                            memoryStream.Position = 0; // Reset the memory stream position after copy
                            
                            //動態命名檔案
                            var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            var newFileName = $"公司版_{timeStamp}_{file.FileName}";

                            var newFilePost = new Files_post
                            {
                                file_name = newFileName,
                                file_type = file.ContentType,
                                post_id = (int)id,
                                post_Type = "公司版",
                                file_size = file.Length,
                                upload_time = DateTime.Now
                            };

                            _context.files_Post.Add(newFilePost);

                            //指定位置儲存
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PostFile", newFileName);
                            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                            {
                                memoryStream.WriteTo(fileStream);
                            }
                        }
                    }
                }
                await _context.SaveChangesAsync();
                TempData["SuccessfulEdit"] = true;
                if (HttpContext.Session.GetString("User") == null)
                {   
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login","Member");
                }
                else
                {
                    return RedirectToAction(nameof(Post_Version));
                }
            }
            var files = await _context.files_Post.Where(fp => fp.post_id == id && fp.post_Type == "公司版").ToListAsync();
            model.Files_posts = files;
            return View(model);
        }
               
       

        public async Task<IActionResult> Share_emp_Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emp = await _context.share_emp.FindAsync(id);
            if (emp == null)
            {
                return NotFound();
            }
            return View(emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Share_emp_Edit(int? id, [Bind("id,item,studentID,name,dept,company,location,salary,post,content,emp_status,share_intern")] Share_emp emp)
        {
            ModelState.Remove("dept");
            ModelState.Remove("name");
            ModelState.Remove("studentID");
            if (id != emp.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var originalEmp = await _context.share_emp.AsNoTracking().FirstOrDefaultAsync(e => e.id == id);
                    if (originalEmp == null)
                    {
                        return NotFound();
                    }

                    // Assuming there is a create_time field that you want to keep from the original entry
                    emp.create_time = originalEmp.create_time; // Keep the create time unchanged
                    emp.studentID = originalEmp.studentID;
                    emp.name = originalEmp.name;
                    emp.dept = originalEmp.dept;

                    _context.Update(emp);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShareempExists(emp.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessfulEdit"] = true;
                return RedirectToAction(nameof(Post_Version1));
            }
            return View(emp);
        }

        private bool ShareempExists(int id)
        {
            return _context.share_emp.Any(e => e.id == id);
        }

         public async Task<IActionResult> Share_intern_Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var intern = await _context.share_intern.FindAsync(id);
            if (intern == null)
            {
                return NotFound();
            }

            var files = await _context.files_Post.Where(fp => fp.post_id == id && fp.post_Type == "實習版").ToListAsync();

            var viewModel = new Share_intern_ViewModel
            {
                share_interns = intern,
                Files_posts = files
            };
            return View(viewModel);
        }
        //實習經驗版修改
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Share_intern_Edit(int? id, Share_intern_ViewModel model)
        {   
            //移除學生session驗證
            ModelState.Remove("dept");
            ModelState.Remove("name");
            ModelState.Remove("studentID");
            if (id != model.share_interns?.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var intern = await _context.share_intern.FindAsync(id);//找到share_intern裡的貼文id
                
                if (intern == null)
                {
                    return NotFound();
                }
                //更新修改值
                intern.item = model.share_interns.item;
                intern.year = model.share_interns.year;
                intern.company_name = model.share_interns.company_name;
                intern.location = model.share_interns.location;
                intern.post = model.share_interns.post;
                intern.salary = model.share_interns.salary;
                intern.link = model.share_interns.link;
                intern.work_content = model.share_interns.work_content;
                intern.experience = model.share_interns.experience;
                intern.anonymous = model.share_interns.anonymous;
                _context.Update(intern);

                var existingFiles = await _context.files_Post.Where(fp => fp.post_id == id && fp.post_Type == "實習版").ToListAsync();
                long existingFilesSize = existingFiles.Sum(fp => fp.file_size);
                long newFilesSize = model.NewFiles?.Sum(f => f.Length) ?? 0;
                
                //設定不可超過5MB
                if (existingFilesSize + newFilesSize > 5 * 1024 * 1024)//5MB
                {
                    ModelState.AddModelError(string.Empty, "總檔案大小不能超過 5MB");
                    model.Files_posts = existingFiles;
                    return View(model);
                }

                //刪除在前端所選的資料
                if (model.FilesToDelete != null)
                {
                    var filesToDelete = existingFiles.Where(fp => model.FilesToDelete.Contains(fp.Id)).ToList();
                    foreach (var file in filesToDelete)
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PostFile", file.file_name);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);//刪除本地端的檔案
                        }
                        _context.files_Post.Remove(file);//刪除files_Post資料
                    }
                }
                if (model.NewFiles != null)//新增檔案
                {
                    foreach (var file in model.NewFiles)
                    {
                        var allowedFileTypes = new[] { "application/pdf", "image/jpeg", "image/png" };//設定
                        if (!allowedFileTypes.Contains(file.ContentType))
                        {
                            ModelState.AddModelError(string.Empty, "只能上傳 PDF 或圖片檔案。");
                            model.Files_posts = existingFiles;
                            return View(model);
                        }
                        if (file.Length > 0)
                        {
                            var memoryStream = new MemoryStream();
                            await file.CopyToAsync(memoryStream);
                            memoryStream.Position = 0; // Reset the memory stream position after copy
                            
                            // Dynamically name the file
                            var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            var newFileName = $"實習版_{timeStamp}_{file.FileName}";

                            var newFilePost = new Files_post
                            {
                                file_name = newFileName,
                                file_type = file.ContentType,
                                post_id = (int)id,
                                post_Type = "實習版",
                                file_size = file.Length,
                                upload_time = DateTime.Now
                            };

                            _context.files_Post.Add(newFilePost);

                            // Save the file locally
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PostFile", newFileName);
                            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                            {
                                memoryStream.WriteTo(fileStream);
                            }
                        }
                    }
                }
                await _context.SaveChangesAsync();
                TempData["SuccessfulEdit"] = true;
                return RedirectToAction(nameof(Post_Version2));
            }
            var files = await _context.files_Post.Where(fp => fp.post_id == id && fp.post_Type == "實習版").ToListAsync();
            model.Files_posts = files;
            return View(model);
        }   
        
        
        //按讚      
        [HttpPost]
        public ActionResult Thumb_like(int id, string postType)
        {
            var user = HttpContext.Session.GetString("User");
            var data = JsonConvert.DeserializeObject<Member>(user);
            var MemberID = _context.member.Find(data.id);
            var like = _context.thumb.FirstOrDefault(l => l.student_Id == MemberID.studentID && l.post_id == id.ToString() && l.post_Type == postType);

            bool isLiked = like != null;//Thumb有找到對應資料是True

            int likeCount = 0;//初始值
            if (like == null)//按讚
            {
                var thumb = new Thumb
                {
                    post_id = id.ToString(),
                    post_Type = postType,
                    student_Id = MemberID.studentID,
                    student_Name = MemberID.student_name,
                    like_click_time = DateTime.Now
                };
                
                _context.thumb.Add(thumb);//新增
                string MemberName = MemberID.student_name;
                string Name = MemberID.student_name.Length < 3 ? MemberID.student_name.Substring(0, 1) 
                                                                    : MemberID.student_name[0] + new string('○', MemberID.student_name.Length - 2) + MemberID.student_name[^1];//名字中間變○，ex.王○明

                //針對不同版去做處理
                if(postType == "公司版"){
                    var internPost = _context.find_intern.FirstOrDefault(p => p.id == id);
                    var Author = _context.member.FirstOrDefault(p => p.Level == "管理員");
                    internPost.like += 1;
                    _context.SaveChanges();
                    _lineNotifyService.SendNotify(Author.AccessToken, $"{Name}在公司實習版按讚了《{internPost.item}》貼文",null,null);
                }
                else if(postType == "就業版"){
                    var internPost = _context.share_emp.FirstOrDefault(p => p.id == id);
                    var Author = _context.member.FirstOrDefault(p => p.studentID == internPost.studentID);
                    internPost.like += 1;
                    _context.SaveChanges();
                    _lineNotifyService.SendNotify(Author.AccessToken, $"{Name}在就業分享版按讚了《{internPost.item}》貼文",null,null);
                }
                else{
                    var internPost = _context.share_intern.FirstOrDefault(p => p.id == id);
                    var Author = _context.member.FirstOrDefault(p => p.studentID == internPost.studentID);
                    internPost.like += 1;
                    _context.SaveChanges();
                    _lineNotifyService.SendNotify(Author.AccessToken, $"{Name}在實習經驗版按讚了《{internPost.item}》貼文",null,null);
                }
            }
             else//收回讚
            {
                _context.thumb.Remove(like);
                _context.SaveChanges();

                if(postType == "公司版"){
                    var internPost = _context.find_intern.FirstOrDefault(p => p.id == id);
                    internPost.like = Math.Max(0, internPost.like - 1);//防止負號
                    _context.SaveChanges();
                }
                else if(postType == "就業版"){
                    var internPost = _context.share_emp.FirstOrDefault(p => p.id == id);
                    internPost.like = Math.Max(0, internPost.like - 1);//防止負號
                    _context.SaveChanges();
                }
                else{
                    var internPost = _context.share_intern.FirstOrDefault(p => p.id == id);
                    internPost.like = Math.Max(0, internPost.like - 1);//防止負號
                    _context.SaveChanges();
                }
            }
            likeCount = _context.thumb.Count(m => m.post_id == id.ToString() && m.post_Type == postType);
            ViewBag.IsLiked = isLiked;
            return Json(new { success = true, likeCount = likeCount, isLiked = isLiked });
        }

        //收藏
        [HttpPost]
        public ActionResult Collection_keep(int id, string postType)
        {
            var user = HttpContext.Session.GetString("User");
            var data = JsonConvert.DeserializeObject<Member>(user);
            var MemberID = _context.member.Find(data.id);
            var keep = _context.collection.FirstOrDefault(l => l.student_Id == MemberID.studentID && l.post_id == id.ToString() && l.post_Type == postType);

            bool iskeeped = keep != null;//keep有找到對應資料是True

            int keepCount = 0;
            if (keep == null)//按珍藏
            {
                var Collection = new Collection
                {
                    post_id = id.ToString(),
                    post_Type = postType,
                    student_Id = MemberID.studentID,
                    student_Name = MemberID.student_name,
                    collection_click_time = DateTime.Now
                };
                _context.collection.Add(Collection);//新增
                
                //針對不同版去做處理
                if(postType == "公司版"){
                    var internPost = _context.find_intern.FirstOrDefault(p => p.id == id);
                    internPost.keep += 1;
                    _context.SaveChanges();
                }
                else if(postType == "就業版"){
                    var internPost = _context.share_emp.FirstOrDefault(p => p.id == id);
                    internPost.keep += 1;
                    _context.SaveChanges();
                }
                else{
                    var internPost = _context.share_intern.FirstOrDefault(p => p.id == id);
                    internPost.keep += 1;
                    _context.SaveChanges();
                }
            }
            else //收回珍藏
            {
                _context.collection.Remove(keep);//移除
                _context.SaveChanges();

                //針對不同版去做處理
                if(postType == "公司版"){
                    var internPost = _context.find_intern.FirstOrDefault(p => p.id == id);
                    internPost.keep = Math.Max(0, internPost.keep - 1);//防止負號
                    _context.SaveChanges();
                }
                else if(postType == "就業版"){
                    var internPost = _context.share_emp.FirstOrDefault(p => p.id == id);
                    internPost.keep = Math.Max(0, internPost.keep - 1);//防止負號
                    _context.SaveChanges();
                }
                else{
                    var internPost = _context.share_intern.FirstOrDefault(p => p.id == id);
                    internPost.keep = Math.Max(0, internPost.keep - 1);//防止負號
                    _context.SaveChanges();
                }
            }
            keepCount = _context.collection.Count(m => m.post_id == id.ToString() && m.post_Type == postType);

            return Json(new { success = true, keepCount = keepCount, iskeeped = iskeeped });
        }
        
        //PDF
        public IActionResult GetPDF(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PostFile", fileName);
            if (System.IO.File.Exists(filePath))
            {
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/pdf");
            }
            return NotFound();
        }
    }
}