using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace sssmtpServer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        private void testSend()
        {
            // TBW
        }

        public void OnGet()
        {

        }
        public void OnPost(string labelValue)
        {
            if (labelValue == "投稿テスト") testSend();
            else
            {

            }
        }
    }

}