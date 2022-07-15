using System.Diagnostics;
using System.Net.Mail;
using System.Text;
using Tweetinvi;

const string from = @"C:\ProgramData\autumn\autumnTwitterBot001";

StringBuilder report = new StringBuilder();

string? consumerKey;
string? consumerSecret;
string? accessToken;
string? accessTokenSecret;

bool testMode = false;
if( args.Length > 0)
{
    testMode = args[0].Contains("test");
}

for (int i = 1; ; i++)
{
    try
    {
        report.AppendLine($"order={i}");
        var src = Path.Combine(from, i.ToString() + "_authinfo.txt");
        if (!File.Exists(src)) break;
        // read seed & counter
        int seed = 0, counter = 0;
        var seedFileName = Path.Combine(from, i.ToString() + "_seed.txt");
        var counterFileName = Path.Combine(from, i.ToString() + "_counter.txt");
        if (File.Exists(seedFileName)) int.TryParse(File.ReadAllText(seedFileName), out seed);
        if (File.Exists(counterFileName)) int.TryParse(File.ReadAllText(counterFileName), out counter);
        report.AppendLine($"seed={seed}");
        report.AppendLine($"counter={counter}");
        // read auth info
        loadInfo(src);
        // read base data
        string[] lines = loadData(Path.Combine(from, i.ToString() + "_data.txt"));
        // fix seed & counter
        if (seed == 0 || counter >= lines.Length)
        {
            report.AppendLine("Reseting");
            seed = Random.Shared.Next(1, int.MaxValue);
            counter = 1;
            File.WriteAllText(seedFileName, seed.ToString());
            report.AppendLine($"seed={seed}");
            report.AppendLine($"counter={counter}");
        }
        // create table
        var random = new Random(seed);
        var table = Enumerable.Range(0, lines.Length).ToArray();
        for (int j = 0; j < lines.Length; j++)
        {
            var k = random.Next(0, lines.Length - 1);
            (table[j], table[k]) = (table[k], table[j]);
        }
        var targetLine = lines[counter++];
        File.WriteAllText(counterFileName, counter.ToString());
        await tweet(consumerKey, consumerSecret, accessToken, accessTokenSecret, targetLine);
        report.AppendLine($"Tweeted: {targetLine}");
    }
    catch(Exception e)
    {
        report.Append(e.ToString());
    }
}

if (testMode)
{
    var filename = Path.Combine(from, "test.txt");
    File.WriteAllText(filename, report.ToString());
    var startInfo = new System.Diagnostics.ProcessStartInfo(filename);
    startInfo.UseShellExecute = true;
    System.Diagnostics.Process.Start(startInfo);
}
else
{
    var filename = Path.Combine(from, "mail.txt");
    var mailAddress = File.ReadAllText(filename);
    SmtpClient client = new SmtpClient("127.0.0.1");
    client.Send(mailAddress, mailAddress, "autumnTwitterBot001 report " + DateTime.Now.ToString("yyMMddHHmmss"), report.ToString());
}

string[] loadData(string filename)
{
    var lines = new List<string>();
    using var reader = File.OpenText(filename);
    for (; ; )
    {
        var s = reader.ReadLine();
        if (s == null) break;
        s = s.Trim();
        if (s.Length == 0) continue;
        if (!s.ToLower().Contains("http"))
        {
            report.AppendLine($"WARING: {s} is not contains 'http'");
            continue;
        }
        lines.Add(s);
    }
    return lines.ToArray();
}

void loadInfo(string from)
{
    using var reader = File.OpenText(from);
    consumerKey = reader.ReadLine();
    consumerSecret = reader.ReadLine();
    accessToken = reader.ReadLine();
    accessTokenSecret = reader.ReadLine();
}

async Task tweet(string? consumerKey, string? consumerSecret, string? accessToken, string? accessTokenSecret, string targetLine)
{
    var userClient = new TwitterClient(consumerKey, consumerSecret, accessToken, accessTokenSecret);
    //var user = await userClient.Users.GetAuthenticatedUserAsync();
    //Console.WriteLine(user);
    if (!testMode)
    {
        var tweet = await userClient.Tweets.PublishTweetAsync(targetLine);
    }
}

