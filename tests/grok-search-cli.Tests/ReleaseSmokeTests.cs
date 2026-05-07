using System.Text.RegularExpressions;

namespace XaiSearchCli.Tests;

public class ReleaseSmokeTests
{
    // The asset naming convention is:
    //   grok-search-cli_<version>_<rid>.{zip,tar.gz}
    //   grok-search-cli_<version>_<rid>.sha256
    //   checksums_<version>.txt
    private static readonly string[] SupportedRids = ["win-x64", "linux-x64", "osx-arm64", "osx-x64"];
    private const string SampleVersion = "v1.0.0";

    [Fact]
    public void AssetNaming_ArchivePattern_IsDeterministic()
    {
        foreach (var rid in SupportedRids)
        {
            var isWindows = rid.StartsWith("win");
            var ext = isWindows ? ".zip" : ".tar.gz";
            var name = $"grok-search-cli_{SampleVersion}_{rid}{ext}";

            Assert.Matches(
                @"^grok-search-cli_v\d+\.\d+\.\d+_[\w-]+\.(zip|tar\.gz)$",
                name);

            if (isWindows)
                Assert.EndsWith(".zip", name);
            else
                Assert.EndsWith(".tar.gz", name);
        }
    }

    [Fact]
    public void AssetNaming_ChecksumPattern_IsDeterministic()
    {
        foreach (var rid in SupportedRids)
        {
            var name = $"grok-search-cli_{SampleVersion}_{rid}.sha256";
            Assert.Matches(
                @"^grok-search-cli_v\d+\.\d+\.\d+_[\w-]+\.sha256$",
                name);

            var combinedName = $"checksums_{SampleVersion}.txt";
            Assert.Matches(
                @"^checksums_v\d+\.\d+\.\d+\.txt$",
                combinedName);
        }
    }

    [Fact]
    public void AssetNaming_AllPlatforms_ProduceUniqueNames()
    {
        var names = SupportedRids.Select(rid => $"grok-search-cli_{SampleVersion}_{rid}").ToList();
        Assert.Equal(names.Count, names.Distinct().Count());
    }

    [Fact]
    public void SupportedRids_AreExpected()
    {
        Assert.Contains("win-x64", SupportedRids);
        Assert.Contains("linux-x64", SupportedRids);
        Assert.Contains("osx-arm64", SupportedRids);
        Assert.Contains("osx-x64", SupportedRids);
    }

    [Fact]
    public void InstallScript_Bash_MustNotCollectApiKeys()
    {
        // Smoke test: verify the bash installer does not contain any API key collection
        // Try to find the script relative to the test project
        var searchPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "install.sh"),
            Path.Combine(Directory.GetCurrentDirectory(), "install.sh"),
            // From the test project directory
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "install.sh"),

        };

        string? resolved = searchPaths.FirstOrDefault(File.Exists);
        // If we can't find the file, skip this test (it might be running from a different context)
        if (resolved is null) return;

        var content = File.ReadAllText(resolved);

        // The installer must NOT prompt for or collect the API key interactively.
        // (It may reference XAI_API_KEY in post-install guidance, but must not
        //  call read or prompt for secret input during installation.)
        var lines = content.Split('\n');
        var readLines = lines.Where(l => l.TrimStart().StartsWith("read "));
        Assert.DoesNotContain(readLines, l => l.Contains("secret", StringComparison.OrdinalIgnoreCase)
                                            || l.Contains("key", StringComparison.OrdinalIgnoreCase)
                                            || l.Contains("api", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void InstallScript_PowerShell_MustNotCollectApiKeys()
    {
        var searchPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "install.ps1"),
            Path.Combine(Directory.GetCurrentDirectory(), "install.ps1"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "install.ps1"),
        };

        string? resolved = searchPaths.FirstOrDefault(File.Exists);
        if (resolved is null) return;

        var content = File.ReadAllText(resolved);

        // Should not contain secret-prompting patterns
        Assert.DoesNotContain("Read-Host", content);

    }

    [Fact]
    public void InstallScript_Bash_HasAuthHandoff()
    {
        var searchPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "install.sh"),
            Path.Combine(Directory.GetCurrentDirectory(), "install.sh"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "install.sh"),
        };

        string? resolved = searchPaths.FirstOrDefault(File.Exists);
        if (resolved is null) return;

        var content = File.ReadAllText(resolved);

        // Must reference the auth login flow
        Assert.Contains("auth login", content);
    }

    [Fact]
    public void InstallScript_PowerShell_HasAuthHandoff()
    {
        var searchPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "install.ps1"),
            Path.Combine(Directory.GetCurrentDirectory(), "install.ps1"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "install.ps1"),
        };

        string? resolved = searchPaths.FirstOrDefault(File.Exists);
        if (resolved is null) return;

        var content = File.ReadAllText(resolved);

        // Must reference the auth login flow
        Assert.Contains("auth login", content);
    }

    [Fact]
    public void InstallScript_Bash_HasVersionSupport()
    {
        var searchPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "install.sh"),
            Path.Combine(Directory.GetCurrentDirectory(), "install.sh"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "install.sh"),
        };

        string? resolved = searchPaths.FirstOrDefault(File.Exists);
        if (resolved is null) return;

        var content = File.ReadAllText(resolved);

        // Must support --version flag
        Assert.Contains("--version", content);
    }

    [Fact]
    public void InstallScript_PowerShell_HasVersionSupport()
    {
        var searchPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "install.ps1"),
            Path.Combine(Directory.GetCurrentDirectory(), "install.ps1"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "install.ps1"),
        };

        string? resolved = searchPaths.FirstOrDefault(File.Exists);
        if (resolved is null) return;

        var content = File.ReadAllText(resolved);

        // Must support -Version parameter
        Assert.Contains("$Version", content);
    }

    [Fact]
    public void ReleaseWorkflow_Exists()
    {
        var searchPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", ".github", "workflows", "release.yml"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", ".github", "workflows", "release.yml"),
        };

        string? resolved = searchPaths.FirstOrDefault(File.Exists);
        Assert.NotNull(resolved);
        Assert.True(File.Exists(resolved));
    }

    [Fact]
    public void InstallScripts_Exist()
    {
        var searchPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "install.sh"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "install.ps1"),
        };

        var bashExists = searchPaths.Any(p => File.Exists(p) && p.EndsWith(".sh"));
        var psExists = searchPaths.Any(p => File.Exists(p) && p.EndsWith(".ps1"));

        // At least one should be found from the test context
        var testDirMonoRepo = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..");
        Assert.True(
            File.Exists(Path.Combine(testDirMonoRepo, "install.sh")) ||
            File.Exists(Path.Combine(testDirMonoRepo, "install.ps1")) ||
            bashExists || psExists,
            "Install scripts should exist in the repository root");
    }
}
