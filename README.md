# What is this?
Vbox7DL is a **C#** Command Line Tool that allows users to download videos from [Vbox7](https://www.vbox7.com/). It was created as a solution for the inability of other tools, such as youtube-dl and yt-dlp, to download videos from the [Vbox7](https://www.vbox7.com/) website.

# Arguments
Vbox7 uses 2 arguments
```
-i, --input <input> (REQUIRED) A URL link or Video ID to a Vbox7 video

-o, --output <output> (Default: Current Directory)
```

# Usage
**Example 1:**
```
-i https://www.vbox7.com/play:6d16e1524a -o C:\Users\Admin\Videos\
```
You can download multiple videos at once by using the following command:
**Example 2:**
```
-i https://www.vbox7.com/play:6d16e1524a https://www.vbox7.com/play:6feba799
```

# How to run Vbox7DL?
You can use ``dotnet run -- -i https://www.vbox7.com/play:6d16e1524a``, compile a binary using [dotnet publish](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-publish) or download a compiled version from the [Releases](https://github.com/DontEatOreo/Vbox7DL/releases) tab.

# Nuget Packages
```
CliWrap
System.CommandLine
```