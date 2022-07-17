using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using sssmtpSenderLib;

namespace sssmtpServer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        private async Task testSendAsync()
        {
            string myUrl = Request.Scheme + "://" + Request.Host + Request.Path.Value;
            Sender.Init(new SendInfoConstant(myUrl));
            await Sender.SendAsync("sssmtpServerテスター",DateTimeOffset.Now.ToString("yyyyMMddHHmmss")+ "作成のテストデータ。The quick brown fox jumped over the lazy dog. すばしっこい茶色のキツネが、怠け者の犬を飛び越えました。");
        }

        public void OnGet()
        {

        }
        public async void OnPost(string labelValue)
        {
            if (labelValue == "投稿テスト") await testSendAsync();
            else  if (labelValue == "MESSAGEPOST")
            {
                System.Diagnostics.Debug.WriteLine("HERE!");
                // TBW
            }
            else
            {

            }
        }
    }
}
