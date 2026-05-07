using XaiSearchCli.Configuration;

namespace XaiSearchCli;

public static class AuthCommand
{
    private const string EnvKey = "XAI_API_KEY";

    public static void Handle(string[] args, TextWriter stderr)
    {
        if (args.Length == 0)
        {
            stderr.WriteLine("error: expected auth subcommand: 'login', 'status', or 'logout'");
            Environment.Exit(1);
            return;
        }

        switch (args[0])
        {
            case "login":
                HandleLogin(args[1..], stderr);
                break;
            case "status":
                HandleStatus(stderr);
                break;
            case "logout":
                HandleLogout(stderr);
                break;
            default:
                stderr.WriteLine($"error: unknown auth subcommand '{args[0]}'. Expected 'login', 'status', or 'logout'");
                Environment.Exit(1);
                break;
        }
    }

    private static void HandleLogin(string[] args, TextWriter stderr)
    {
        var useStdin = args.Contains("--api-key-stdin");

        string apiKey;

        if (useStdin)
        {
            if (!Console.IsInputRedirected)
            {
                stderr.WriteLine("error: --api-key-stdin requires stdin to be redirected");
                Environment.Exit(1);
                return;
            }

            apiKey = (Console.In.ReadToEnd() ?? "").Trim();
        }
        else if (!Console.IsInputRedirected)
        {
            apiKey = ReadSecretFromConsole(stderr);
        }
        else
        {
            stderr.WriteLine("error: stdin is redirected but --api-key-stdin was not provided. " +
                             "Use --api-key-stdin for non-interactive input, or run 'auth login' " +
                             "in an interactive terminal.");
            Environment.Exit(1);
            return;
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            stderr.WriteLine("error: API key cannot be empty");
            Environment.Exit(1);
            return;
        }

        CredentialStore.Write(apiKey);
        stderr.WriteLine("API key saved to managed credential store.");
    }

    private static string ReadSecretFromConsole(TextWriter stderr)
    {
        stderr.Write("Enter your xAI API key: ");
        var chars = new List<char>();

        while (true)
        {
            var keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                stderr.WriteLine();
                break;
            }

            if (keyInfo.Key == ConsoleKey.Backspace && chars.Count > 0)
            {
                chars.RemoveAt(chars.Count - 1);
                continue;
            }

            if (keyInfo.KeyChar != 0 && !char.IsControl(keyInfo.KeyChar))
                chars.Add(keyInfo.KeyChar);
        }

        return new string(chars.ToArray());
    }

    private static void HandleStatus(TextWriter stderr)
    {
        // Check sources in precedence order: env, .env, managed store
        var envKey = Environment.GetEnvironmentVariable(EnvKey);
        if (!string.IsNullOrEmpty(envKey))
        {
            stderr.WriteLine("status: configured via XAI_API_KEY environment variable");
            return;
        }

        var (dotEnvKey, dotEnvPath) = LoadFromDotEnvWithPath();
        if (!string.IsNullOrEmpty(dotEnvKey))
        {
            stderr.WriteLine($"status: configured via .env file ({dotEnvPath})");
            return;
        }

        var storeKey = CredentialStore.Read();
        if (!string.IsNullOrEmpty(storeKey))
        {
            stderr.WriteLine("status: configured via managed credential store");
            return;
        }

        stderr.WriteLine("status: not configured. Run 'grok-search-cli auth login' to set up credentials.");
    }

    private static (string? key, string? path) LoadFromDotEnvWithPath()
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
                    if (trimmed.Length == 0 || trimmed[0] == '#') continue;

                    var eq = trimmed.IndexOf('=');
                    if (eq < 0) continue;

                    var key = trimmed[..eq].Trim();
                    if (key != EnvKey) continue;

                    var val = trimmed[(eq + 1)..].Trim().Trim('"', '\'');
                    if (val.Length > 0)
                        return (val, Path.GetFullPath(path));
                }
            }

            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }

        return (null, null);
    }

    private static void HandleLogout(TextWriter stderr)
    {
        if (CredentialStore.Exists())
        {
            CredentialStore.Delete();
            stderr.WriteLine("managed credential store cleared.");
        }
        else
        {
            stderr.WriteLine("no managed credentials found. Nothing to remove.");
        }

        var envKey = Environment.GetEnvironmentVariable(EnvKey);
        if (!string.IsNullOrEmpty(envKey))
        {
            stderr.WriteLine("note: XAI_API_KEY is still set in the environment. Search will continue to use it.");
        }
        else
        {
            var (dotEnvKey, _) = LoadFromDotEnvWithPath();
            if (!string.IsNullOrEmpty(dotEnvKey))
            {
                stderr.WriteLine("note: XAI_API_KEY is still set in a .env file. Search will continue to use it.");
            }
        }
    }
}
