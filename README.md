# NuGetFeed

A tiny ASP.NET Core Web API site that provides an RSS feed for NuGet package versions.

## Use

http://nugetfeed.ganss.org/versions/NUnit or

http://nugetfeed.ganss.org/versions/NUnit?prerelease=true

## Deploy

When deploying on Windows/IIS you need to give `IIS_IUSRS` permission to read/write `%SystemRoot%\System32\config\systemprofile\AppData\{Local,Roaming}`. 
If you know how this can be avoided, please let me know.
