using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_web.Services
{
    public interface ILineNotifyService
    {
        Task SendNotify(string? accessToken, string message, int? stickerPackageId, int? stickerId);
    }
    public class LineNotifyService : ILineNotifyService
    {
        public async Task SendNotify(string? accessToken, string message, int? stickerPackageId, int? stickerId)
        {
            //使用HttpClient向LINE Notify發送通知
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("message", message),//訊息
                    new KeyValuePair<string, string>("stickerPackageId", stickerPackageId.ToString()),//貼圖種類的PackageId
                    new KeyValuePair<string, string>("stickerId", stickerId.ToString())//選定哪個貼圖的id
                });

                await client.PostAsync("https://notify-api.line.me/api/notify", content);
            }
        }
    }
}