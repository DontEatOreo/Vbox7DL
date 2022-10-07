# What is this?
Vbox7DL is a C# Command Line Tool which is able to download videos from [Vbox7](https://www.vbox7.com/) it was created due to inability of `youtube-dl` and `yt-dlp` to download videos from [Vbox7](https://www.vbox7.com/)

# Arguments
Vbox7 has at the moment 2 arugments
```
-i, --input <input> (REQUIRED) A URL link or Video ID to a Vbox7 video

-o, --output <output> (Default: Current Directory)
```

# Usage
**Example 1:**
```
-i https://www.vbox7.com/play:6d16e1524a -o C:\Users\Admin\Downloads\
```
**Example 2:**
```
--input 6d16e1524a
```

# How to run Vbox7DL?
You can use `dotnet run -- -i https://www.vbox7.com/play:6d16e1524a` or compile a binary using [dotnet publish](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-publish)

# Nuget Packages
```
System.CommandLine
Xabe.FFmpeg
YoutubeDLSharp
```