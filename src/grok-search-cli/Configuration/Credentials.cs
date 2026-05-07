namespace XaiSearchCli.Configuration;

public static class Credentials
{
    private const string EnvKey = "XAI_API_KEY";

    public static string Resolve(TextWriter errorOutput)
    {
        var key = Environment.GetEnvironmentVariable(EnvKey);
        if (!string.IsNullOrEmpty(key))
            return key;

        key = LoadFromDotEnv();
        if (!string.IsNullOrEmpty(key))
            return key;

        key = CredentialStore.Read();
        if (!string.IsNullOrEmpty(key))
            return key;

        errorOutput.WriteLine("error: XAI_API_KEY is not set. Provide it via the environment variable, a .env file, or run 'grok-search-cli auth login'.");
        Environment.Exit(1);
        return null!; // unreachable
    }

    private static string? LoadFromDotEnv()
    {
        var dir = Directory.GetCurrentDirectory();
        while (dir != null)
        {
            var path = Path.Combine(dir, ".env");
            if (File.Exists(path))
            {
                foreach (var line in File.ReadAllLines(path))
                {
                    var trimmed = line.Trim();
                    if (trimmed.Length == 0 || trimmed[0] == '#')
                        continue;

                    var eq = trimmed.IndexOf('=');
                    if (eq < 0) continue;

                    var key = trimmed[..eq].Trim();
                    if (key != EnvKey) continue;

                    var val = trimmed[(eq + 1)..].Trim().Trim('"', '\'');
                    if (val.Length > 0)
                        return val;
                }
            }

            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }

        return null;
    }
}
