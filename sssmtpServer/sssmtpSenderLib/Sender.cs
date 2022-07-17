using System.Net.Http;
using System.Xml.Linq;

namespace sssmtpSenderLib
{
    public class SendInfo
    {
        public string? sendingToUri { get; set; }
        public string? authId { get; set; }
        public string? authPassword { get; set; }
    }
    public class SendInfoFromFile : SendInfo
    {
        private const string src = @"C:\ProgramData\autumn\sssmtpSenderLib\auth.txt";
        public SendInfoFromFile()
        {
            using var reader = File.OpenText(src);
            sendingToUri = reader.ReadToEnd();
            authId = reader.ReadToEnd();
            authPassword = reader.ReadToEnd();
        }
    }
    public class SendInfoConstant : SendInfoFromFile
    {
        public SendInfoConstant(string uri) : base()
        {
            sendingToUri = uri;
        }
    }

    public static class Sender
    {
        private static readonly HttpClient client = new HttpClient();
        private static SendInfo? sendingInfo;

        public static async Task<string> SendAsync(string senderName, string message)
        {
            var values = new Dictionary<string, string>
            {
                { "name","labelValue" },
                { "value","MESSAGEPOST" },
#if false
                { "senderName", senderName },
                { "date", DateTimeOffset.Now.ToString("yyyyMMddHHmmss") },
                { "message", message }
#endif
            };

            var content = new FormUrlEncodedContent(values);
            //await client.GetAsync(sendingInfo?.sendingToUri);
            //return "";

            var response = await client.PostAsync(sendingInfo?.sendingToUri, content);
            var responseString = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine(response.StatusCode.ToString());
            return responseString;
        }

        public static void Init(SendInfo? info = null)
        {
            if (info == null) info = new SendInfoFromFile();
            sendingInfo = info;
        }
    }
}
