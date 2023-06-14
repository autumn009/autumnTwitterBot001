using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Mail;
using System.Text;
using Tweetinvi;
using Tweetinvi.Core.Web;
using Tweetinvi.Models;

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
        var src = Path.Combine(from, i.ToString() + "_authinfo.txt");
        if (!File.Exists(src)) break;
        report.AppendLine($"order={i}");
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
        var targetLine = lines[table[counter++]];
        File.WriteAllText(counterFileName, counter.ToString());
        report.AppendLine($"Tweeting: {targetLine}");
        await tweet(consumerKey, consumerSecret, accessToken, accessTokenSecret, targetLine);
    }
    catch(Exception e)
    {
        report.Append(e.ToString());
    }
}

var logPath = Path.Combine(from, "Logs");
Directory.CreateDirectory(logPath);
var filename = Path.Combine(logPath, "autumnTwitterBot001_" + DateTime.Now.ToString("yyMMddHHmmss") + ".txt");
File.WriteAllText(filename, report.ToString());
if (testMode)
{
    var startInfo = new System.Diagnostics.ProcessStartInfo(filename);
    startInfo.UseShellExecute = true;
    System.Diagnostics.Process.Start(startInfo);
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
        var poster = new TweetsV2Poster(userClient);

        ITwitterResult result = await poster.PostTweet(
            new TweetV2PostRequest
            {
                Text = targetLine
            }
        );

        if (result.Response.IsSuccessStatusCode == false)
        {
            throw new Exception(
                "Error when posting tweet: " + Environment.NewLine + result.Content
            );
        }

    }
}

// from https://github.com/linvi/tweetinvi/issues/1147
public class TweetsV2Poster
{
    // ----------------- Fields ----------------

    private readonly ITwitterClient client;

    // ----------------- Constructor ----------------

    public TweetsV2Poster(ITwitterClient client)
    {
        this.client = client;
    }

    public Task<ITwitterResult> PostTweet(TweetV2PostRequest tweetParams)
    {
        return this.client.Execute.AdvanceRequestAsync(
            (ITwitterRequest request) =>
            {
                var jsonBody = this.client.Json.Serialize(tweetParams);

                // Technically this implements IDisposable,
                // but if we wrap this in a using statement,
                // we get ObjectDisposedExceptions,
                // even if we create this in the scope of PostTweet.
                //
                // However, it *looks* like this is fine.  It looks
                // like Microsoft's HTTP stuff will call
                // dispose on requests for us (responses may be another story).
                // See also: https://stackoverflow.com/questions/69029065/does-stringcontent-get-disposed-with-httpresponsemessage
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                request.Query.Url = "https://api.twitter.com/2/tweets";
                request.Query.HttpMethod = Tweetinvi.Models.HttpMethod.POST;
                request.Query.HttpContent = content;
            }
        );
    }
}

/// <summary>
/// There are a lot more fields according to:
/// https://developer.twitter.com/en/docs/twitter-api/tweets/manage-tweets/api-reference/post-tweets
/// but these are the ones we care about for our use case.
/// </summary>
public class TweetV2PostRequest
{
    /// <summary>
    /// The text of the tweet to post.
    /// </summary>
    [JsonProperty("text")]
    public string Text { get; set; } = string.Empty;
}
