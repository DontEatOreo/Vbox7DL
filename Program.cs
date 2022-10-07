using System.CommandLine;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Vbox7DL;
using Xabe.FFmpeg;
using YoutubeDLSharp;

HttpClient client = new();

YoutubeDL youtubeDl = new()
{
    YoutubeDLPath = "yt-dlp",
    FFmpegPath = "ffmpeg",
    OutputFileTemplate = "%(id)s.%(ext)s",
    OverwriteFiles = false,
    IgnoreDownloadErrors = false
};

var separatorChar = Path.DirectorySeparatorChar;

Option<string> inputOption = new(new[] {"-i", "--input"}, "A URL link or Video ID to a Vbox7 video")
{
    AllowMultipleArgumentsPerToken = false,
    IsRequired = true,
    Arity = default
};

Option<string> outputOption = new(new [] { "-o", "--output" }, "The output file to write to")
{
    AllowMultipleArgumentsPerToken = false,
    IsRequired = false,
    Arity = default,
};
outputOption.SetDefaultValue(Directory.GetCurrentDirectory());

RootCommand rootCommand = new("VBOX7 Downloader");
foreach (var option in new[] { inputOption, outputOption })
{
    rootCommand.AddOption(option);
}

rootCommand.SetHandler(VideoHandler, inputOption, outputOption);

async Task VideoHandler(string inputValue, string outputValue)
{
    if (string.IsNullOrEmpty(youtubeDl.FFmpegPath))
    {
        Console.WriteLine("FFmpeg is not in the path\nYou can download ffmpeg from https://ffmpeg.org/download.html");
        return;
    }

    if (string.IsNullOrEmpty(youtubeDl.YoutubeDLPath))
    {
        Console.WriteLine("yt-dlp is not in the path\nYou can download yt-dlp from https://github.com/yt-dlp/yt-dlp");
        return;
    }

    if (!inputValue.Contains("https://") && Regex.IsMatch(inputValue, @"^[a-z0-9]{10}$"))
    {
        inputValue = $"https://www.vbox7.com/play:{inputValue}";
    }
    
    var videoId = new Regex(@"play:(?<id>\w+)").Match(inputValue);
    await DownloadVideo(client, videoId, outputValue);
}

async Task DownloadVideo(HttpClient httpClient, Match match, string outputValue) 
{
    youtubeDl.OutputFolder = outputValue;
    var response =
        await httpClient.GetFromJsonAsync<Vbox7Json.Root>(
            $"https://www.vbox7.com/aj/player/video/options?vid={match.Groups["id"].Value}");

    if (!response!.Success)
    {
        Console.WriteLine("Invalid URL or ID");
        throw new Exception("Invalid URL or ID");
    }
    var src = response.Options.Src;
    var resolution = response.Options.HighestRes;

    // If the video has been downloaded before it will be deleted
    if (File.Exists($"{outputValue}{separatorChar}{response.Options.Title}.mp4"))
    {
        File.Delete($"{outputValue}{separatorChar}{response.Options.Title}.mp4");
    }

    var videoUrl = src.Replace(".mpd", $"_{resolution}_track1_dash.mp4");
    var audioUrl = src.Replace(".mpd", $"_{resolution}_track2_dashinit.mp4");

    var videoProgress = new Progress<DownloadProgress>(p =>
    {
        if (p.Progress == 0) return;
        Console.Write($"\rVideo Progress: {p.Progress:P}");
    });
    try
    {
        await youtubeDl.RunVideoDownload(videoUrl, progress: videoProgress);
    }
    catch (Exception)
    {
        Console.WriteLine("Could not download video");
        throw;
    }

    Console.WriteLine();

    var audioProgress = new Progress<DownloadProgress>(p =>
    {
        if (p.Progress == 0) return;
        Console.Write($"\rAudio Progress: {p.Progress:P}");
    });
    try
    {
        await youtubeDl.RunVideoDownload(audioUrl, progress: audioProgress);
    }
    catch (Exception)
    {
        Console.WriteLine("Could not download audio");
        throw;
    }
    Console.WriteLine();
    
    var videoFile = new FileInfo($"{outputValue}{separatorChar}{match.Groups["id"].Value}_{resolution}_track1_dash.mp4");
    var audioFile = new FileInfo($"{outputValue}{separatorChar}{match.Groups["id"].Value}_{resolution}_track2_dashinit.mp4");
    var outputFile = new FileInfo($"{outputValue}{separatorChar}{response.Options.Title}.mp4");

    var parameter = $"-i {videoFile} -i {audioFile} -c copy -map 0:v:0 -map 1:a:0 \"{outputFile}\"";
    var conversion = FFmpeg.Conversions.New()
        .AddParameter(parameter);
    try
    {
        await conversion.Start();
    }
    catch (Exception)
    {
        Console.WriteLine("Something went wrong...");
        throw;
    }

    Console.WriteLine($"Video has finished downloading!\n{response.Options.Title} has been downloaded to {outputValue}");

    File.Delete(videoFile.ToString());
    File.Delete(audioFile.ToString());
}

await rootCommand.InvokeAsync(args);