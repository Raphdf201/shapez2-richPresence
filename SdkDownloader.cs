using System.Diagnostics;
using System.Runtime.InteropServices;

namespace shapez2RichPresence;

public static class SdkDownloader
{
    internal static void DownloadSdk()
    {
        using HttpClient client = new();
        // Download the file
        var fileBytes = client.GetByteArrayAsync(GetTargetSdkUrl()).GetAwaiter().GetResult();

        // Save to disk
        File.WriteAllBytes(GetTargetSdkLocation(), fileBytes);
    }

    internal static string GetTargetSdkLocation()
    {
        var path = Process.GetCurrentProcess().MainModule.FileName;
        path = GetOperatingSystem() switch
        {
            OperatingSystem.MacOs => Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(path)), "Plugins"),
            _ => Path.Combine(Path.GetDirectoryName(path), "shapez 2_Data", "Plugins", GetArchitecture() == HwArchitecture.X64 ? "x86_64" : "aarch64")
        };
        path = Path.Combine(path, "discord_game_sdk") ;
        path += GetOperatingSystem() switch
        {
            OperatingSystem.Windows => ".dll",
            OperatingSystem.MacOs => ".dylib",
            _ => ".so"
        };
        return path;
    }

    private static string GetTargetSdkUrl()
    {
        var url = "https://uploads.raphdf201.net/pub/spz2/discordsdk";
        url = Path.Combine(GetArchitecture() == HwArchitecture.X64 ? "/x64" : "/arm", "discord_game_sdk");
        url += GetOperatingSystem() switch
        {
            OperatingSystem.Windows => ".dll",
            OperatingSystem.MacOs => ".dylib",
            _ => ".so"
        };
        return url;
    }

    private static HwArchitecture GetArchitecture()
    {
        var arch = RuntimeInformation.ProcessArchitecture;

        return arch switch
        {
            Architecture.X64 => HwArchitecture.X64,
            Architecture.Arm64 => HwArchitecture.Arm,
            _ => HwArchitecture.Other
        };
    }

    private static OperatingSystem GetOperatingSystem()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return OperatingSystem.Windows;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return OperatingSystem.Linux;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return OperatingSystem.MacOs;

        return OperatingSystem.Other;
    }

    private enum HwArchitecture
    {
        X64,
        Arm,
        Other
    }

    private enum OperatingSystem
    {
        Linux,
        MacOs,
        Windows,
        Other
    }
}
