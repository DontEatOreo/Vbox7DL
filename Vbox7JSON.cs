using System.Text.Json.Serialization;

namespace Vbox7DL;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class Vbox7Json
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Options
    {
        public Options(int highestRes, string title, string src)
        {
            HighestRes = highestRes;
            Title = title;
            Src = src;
        }

        [JsonPropertyName("src")]
        public string Src { get; }

        [JsonPropertyName("title")]
        public string Title { get; }

        [JsonPropertyName("highestRes")]
        public int HighestRes { get; }
    }

    public class Root
    {
        public Root(Options options, bool success)
        {
            Options = options;
            Success = success;
        }

        [JsonPropertyName("success")]
        public bool Success { get; }

        [JsonPropertyName("options")]
        public Options Options { get; }
    }
}