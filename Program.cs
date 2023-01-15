using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using System.Text.RegularExpressions;
using CliWrap;
using Vbox7DL;

var ffmpegCheck = await Cli.Wrap("ffmpeg")
    .WithArguments("-version")
    .WithValidation(CommandResultValidation.ZeroExitCode)
    .ExecuteAsync();
if (ffmpegCheck.ExitCode is not 0)
{
    Console.WriteLine("FFMpeg not found");
    return;
}

HttpClient client = new();

Option<string[]> urlsOption = new(new[] {"-i", "--input"}, "A URL link or Video ID to a Vbox7 video")
{
    AllowMultipleArgumentsPerToken = true,
    IsRequired = true
};

Option<string> outputOption = new(new [] { "-o", "--output" }, "The output file to write to")
{
    IsRequired = false
};
outputOption.SetDefaultValue(Directory.GetCurrentDirectory());

RootCommand rootCommand = new("VBOX7 Downloader");
foreach (var option in new Option[] { urlsOption, outputOption })
    rootCommand.AddOption(option);

rootCommand.SetHandler(VideoHandler);

async Task VideoHandler(InvocationContext invocationContext)
{
    var urls = invocationContext.ParseResult.GetValueForOption(urlsOption)!;
    var outputPath = invocationContext.ParseResult.GetValueForOption(outputOption)!;

    foreach (var url in urls)
    {
        var match = new Regex(@"play:(?<id>\w+)")
            .Match(url);
        await DownloadVideo(match, outputPath);
    }
}

async Task DownloadVideo(Match match, string outputPath)
{
    var response = await client.GetAsync($"https://www.vbox7.com/aj/player/video/options?vid={match.Groups["id"].Value}");
    var jsonResult = JsonSerializer.Deserialize<Vbox7Json.Root>(await response.Content.ReadAsStringAsync())!;

    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine("Invalid URL or ID");
        throw new Exception("Invalid URL or ID");
    }

    var src = jsonResult.Options.Src;
    var resolution = jsonResult.Options.HighestRes;

    // If the video has been downloaded before it will be deleted
    if (Path.Combine(outputPath, $"{jsonResult.Options.Title}.mp4") is { } path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    var videoUrl = src.Replace(".mpd", $"_{resolution}_track1_dash.mp4");
    var videoUrl2 = src.Replace(".mpd", $"_{resolution}_track1_dashinit.mp4");
    var audioUrl = src.Replace(".mpd", $"_{resolution}_track2_dashinit.mp4");

    var videoFile = Path.Combine(outputPath, $"{match.Groups["id"].Value}_{resolution}_track1_dash.mp4");
    var audioFile = Path.Combine(outputPath, $"{match.Groups["id"].Value}_{resolution}_track2_dashinit.mp4");
    var outputFile = Path.Combine(outputPath, $"{jsonResult.Options.Title}.mp4");

    // download video file
    var videoUrlCheck = await client.GetAsync(videoUrl);

    if (videoUrlCheck.IsSuccessStatusCode)
    {
        await using var videoStream = await client.GetStreamAsync(videoUrl);
        await using var videoFileStream = File.Create(videoFile);
        await videoStream.CopyToAsync(videoFileStream);
    }
    else
    {
        await using var videoStream = await client.GetStreamAsync(videoUrl2);
        await using var videoFileStream = File.Create(videoFile);
        await videoStream.CopyToAsync(videoFileStream);
    }

    // download audio file
    await using var audioStream = await client.GetStreamAsync(audioUrl);
    await using var audioFileStream = File.Create(audioFile);
    await audioStream.CopyToAsync(audioFileStream);

    Cli.Wrap("ffmpeg").WithArguments(args => args
        .Add("-i")
        .Add(videoFile)
        .Add("-c copy -map 0:v:0 -map 1:a:0")
        .Add(outputFile))
        .WithValidation(CommandResultValidation.ZeroExitCode)
        .ExecuteAsync();

    Console.WriteLine($"\"{jsonResult.Options.Title}\" has been downloaded to {outputPath}");

    File.Delete(videoFile);
    File.Delete(audioFile);
}

await rootCommand.InvokeAsync(args);