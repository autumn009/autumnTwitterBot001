using System.Diagnostics;
using Tweetinvi;

string? consumerKey;
string? consumerSecret;
string? accessToken;
string? accessTokenSecret;

loadInfo();
await tweet(consumerKey, consumerSecret, accessToken, accessTokenSecret);

void loadInfo()
{
    const string from = @"C:\ProgramData\autumn\autumnTwitterBot001\authinfo.txt";
    using var reader = File.OpenText(from);
    consumerKey = reader.ReadLine();
    consumerSecret = reader.ReadLine();
    accessToken = reader.ReadLine();
    accessTokenSecret = reader.ReadLine();
}

async Task tweet(string? consumerKey, string? consumerSecret, string? accessToken, string? accessTokenSecret)
{
    var userClient = new TwitterClient(consumerKey, consumerSecret, accessToken, accessTokenSecret);
    //var user = await userClient.Users.GetAuthenticatedUserAsync();
    //Console.WriteLine(user);

    var tweet = await userClient.Tweets.PublishTweetAsync("BOTからの投稿テストパート2");
    Console.WriteLine(tweet.Url);
}

