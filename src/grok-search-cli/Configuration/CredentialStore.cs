namespace XaiSearchCli.Configuration;

public static class CredentialStore
{
    private const string FileName = "credentials.env";
    private const string ApiKeyVar = "XAI_API_KEY";

    public static string GetStorePath()
    {
        string baseDir;

        var xdgConfig = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (!string.IsNullOrEmpty(xdgConfig))
        {
            baseDir = xdgConfig;
        }
        else if (OperatingSystem.IsWindows())
        {
            baseDir = Environment.GetEnvironmentVariable("APPDATA")
                     ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
        else
        {
            baseDir = Path.Combine(
                Environment.GetEnvironmentVariable("HOME")
                ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config");
        }

        return Path.Combine(baseDir, "grok-search-cli", FileName);
    }

    public static string? Read()
    {
        var path = GetStorePath();
        if (!File.Exists(path)) return null;

        foreach (var line in File.ReadAllLines(path))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed[0] == '#') continue;

            var eq = trimmed.IndexOf('=');
            if (eq < 0) continue;

            var key = trimmed[..eq].Trim();
            if (key != ApiKeyVar) continue;

            var val = trimmed[(eq + 1)..].Trim().Trim('"', '\'');
            if (val.Length > 0) return val;
        }

        return null;
    }

    public static void Write(string apiKey)
    {
        var path = GetStorePath();
        var dir = Path.GetDirectoryName(path)!;
        Directory.CreateDirectory(dir);
        File.WriteAllText(path, $"{ApiKeyVar}={apiKey}\n");

        if (!OperatingSystem.IsWindows())
        {
            try { File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite); }
            catch { /* best-effort on platforms that do not support SetUnixFileMode */ }
        }
    }

    public static void Delete()
    {
        var path = GetStorePath();
        if (File.Exists(path))
            File.Delete(path);
    }

    public static bool Exists()
    {
        return File.Exists(GetStorePath());
    }
}
