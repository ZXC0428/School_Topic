using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Student_web.Data;
using Student_web.Models;

namespace YourNamespace
{
    public class LineNotifyController : Controller
    {   
        private readonly ApplicationDbContext _context;
        public LineNotifyController(ApplicationDbContext context)
        {
            _context = context;
        }
        //Line Notify服務的設定
        private readonly string clientId = "x2RerdYfjUqRVqeGr9N44P";//Client Id
        private readonly string clientSecret = "uWehct2C6W3HEhECUcbWlWsDxCURRi7Fqy2pQCU60An"; //Client Secret
        private readonly string redirectUri = "https://localhost:7287/LineNotify/Callback";//Callback URL需要跟Line notify裡的一樣

        //授權
        public IActionResult Authenticate()
        {
            string state = "random_string";
            string scope = "notify";
            string authUrl = $"https://notify-bot.line.me/oauth/authorize?response_type=code&client_id={clientId}&redirect_uri={redirectUri}&scope={scope}&state={state}";
            return Redirect(authUrl);//重新導入到Line授權頁面
        }

        [HttpGet("LineNotify/Callback")]
        public async Task<IActionResult> Callback(string code, string state)
        {
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret)
                });

                var response = await client.PostAsync("https://notify-bot.line.me/oauth/token", content);//權杖的連動網址
                var responseString = await response.Content.ReadAsStringAsync();
                var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
                var user = HttpContext.Session.GetString("User");
                if (tokenData.TryGetValue("access_token", out var accessToken))
                {
                   
                    var data = JsonConvert.DeserializeObject<Member>(user);
                    var userID = data.studentID;
                    SaveToken(userID, accessToken);
                    TempData["ShowSuccess"] = true;
                    //重定向到信息頁面
                    return RedirectToAction("MyInformation","Member");
                }

                return BadRequest("無法獲取LINEToken");
            }
        }

        private void SaveToken(string userID, string accessToken)
        {
            var member = _context.member.FirstOrDefault(x => x.studentID == userID);
            member.AccessToken = accessToken;
            member.Updated = DateTime.Now;
            _context.SaveChanges();
        }


        [HttpPost]
        public async Task<IActionResult> RevokeLineNotify()
        {
            //抓member的使用者
            var user = HttpContext.Session.GetString("User");
            var data = JsonConvert.DeserializeObject<Member>(user);
            var userID = _context.member.FirstOrDefault(m => m.studentID ==  data.studentID);

            if(userID.AccessToken!=null && !string.IsNullOrEmpty(userID.AccessToken)){
                var revokeSuccess = await RevokeLineNotifyURL(userID.AccessToken);
                if (revokeSuccess)
                {
                    userID.AccessToken = null;
                    await _context.SaveChangesAsync();
                }
                TempData["ShowRevoke"] = true;
            }
            return RedirectToAction("MyInformation","Member");
        }

        private async Task<bool> RevokeLineNotifyURL(string accessToken)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                var Revoke = await client.PostAsync("https://notify-api.line.me/api/revoke", null);//權杖的解除網址
                return Revoke.IsSuccessStatusCode;
            }
        }
    }
    
}