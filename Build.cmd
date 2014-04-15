@echo off

set framework=v4.0.30319

"%SystemDrive%\Windows\Microsoft.NET\Framework\%framework%\MSBuild.exe" "%~dp0\XamlAttributeOrdering.sln"/Property:Configuration=Release

"%~dp0\packages\NuGet.CommandLine.2.7.3\tools\NuGet.exe" pack "%~dp0\XamlAttributeOrdering\XamlAttributeOrdering\XamlAttributeOrdering.nuspec" -Properties Configuration=Release -Version %1