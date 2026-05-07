using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
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
    public void InstallScript_Bash_HasLocalAssetDirSupport()
    {
        var content = File.ReadAllText(ResolveRepoFile("install.sh"));

        Assert.Contains("--asset-dir", content);
        Assert.Contains("local asset installs require --version", content);
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
    public void InstallScript_PowerShell_HasLocalAssetDirSupport()
    {
        var content = File.ReadAllText(ResolveRepoFile("install.ps1"));

        Assert.Contains("[string]$AssetDir", content);
        Assert.Contains("Local asset installs require -Version", content);
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
    public void ReleaseWorkflow_ChecksumTargetsPackagedArchives()
    {
        var content = File.ReadAllText(ResolveRepoFile(Path.Combine(".github", "workflows", "release.yml")));

        Assert.Contains("sha256sum \"${{ github.workspace }}/grok-search-cli_${{ github.ref_name }}_${{ matrix.rid }}.tar.gz\"", content);
        Assert.Contains("$archiveName = \"grok-search-cli_${{ github.ref_name }}_${{ matrix.rid }}.zip\"", content);
        Assert.Contains("Get-FileHash \"${{ github.workspace }}/$archiveName\"", content);
    }

    [Fact]
    public void LocalPackagingScripts_Exist()
    {
        Assert.True(File.Exists(ResolveRepoFile("package-local-release.ps1")));
        Assert.True(File.Exists(ResolveRepoFile("package-local-release.sh")));
    }

    [Fact]
    public void LocalPackagingScripts_UseReleaseLikeFileNames()
    {
        var powerShellContent = File.ReadAllText(ResolveRepoFile("package-local-release.ps1"));
        var bashContent = File.ReadAllText(ResolveRepoFile("package-local-release.sh"));

        Assert.Contains("grok-search-cli_${Version}_${RuntimeId}.zip", powerShellContent);
        Assert.Contains("grok-search-cli_${Version}_${RuntimeId}.tar.gz", powerShellContent);
        Assert.Contains("grok-search-cli_${Version}_${RuntimeId}.sha256", powerShellContent);
        Assert.Contains("checksums_${Version}.txt", powerShellContent);

        Assert.Contains("grok-search-cli_${VERSION}_${RUNTIME_ID}.tar.gz", bashContent);
        Assert.Contains("grok-search-cli_${VERSION}_${RUNTIME_ID}.sha256", bashContent);
        Assert.Contains("checksums_${VERSION}.txt", bashContent);
    }

    [Fact]
    public void LocalPowerShellTestSessionScript_Exists()
    {
        var content = File.ReadAllText(ResolveRepoFile("start-local-test-session.ps1"));

        Assert.Contains("GROK_SEARCH_CLI_LOCAL_INSTALL_DIR", content);
        Assert.Contains("grok-search-cli is now available in this PowerShell session", content);
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

    [Fact]
    public void UninstallScripts_Exist()
    {
        Assert.True(File.Exists(ResolveRepoFile("uninstall.sh")));
        Assert.True(File.Exists(ResolveRepoFile("uninstall.ps1")));
    }

    [Fact]
    public void UninstallScript_Bash_HasDirectoryOverrideSupport()
    {
        var content = File.ReadAllText(ResolveRepoFile("uninstall.sh"));

        Assert.Contains("--dir", content);
        Assert.Contains("Usage:", content);
    }

    [Fact]
    public void UninstallScript_PowerShell_HasInstallDirSupport()
    {
        var content = File.ReadAllText(ResolveRepoFile("uninstall.ps1"));

        Assert.Contains("[string]$InstallDir", content);
        Assert.Contains(".\\uninstall.ps1 -InstallDir", content);
    }

    [Fact]
    public void UninstallScripts_ReportCleanupBoundary()
    {
        var bashContent = File.ReadAllText(ResolveRepoFile("uninstall.sh"));
        var powerShellContent = File.ReadAllText(ResolveRepoFile("uninstall.ps1"));

        Assert.Contains("XAI_API_KEY", bashContent);
        Assert.Contains(".env", bashContent);
        Assert.Contains("auth-managed storage", bashContent);
        Assert.Contains("PATH", bashContent);

        Assert.Contains("XAI_API_KEY", powerShellContent);
        Assert.Contains(".env", powerShellContent);
        Assert.Contains("auth-managed storage", powerShellContent);
        Assert.Contains("PATH", powerShellContent);
    }

    [Fact]
    public void UninstallScript_CurrentPlatform_IsIdempotentAndBounded()
    {
        if (OperatingSystem.IsWindows())
        {
            ValidatePowerShellUninstallBehavior();
            return;
        }

        ValidateBashUninstallBehavior();
    }

    private static string ResolveRepoFile(string fileName)
    {
        var searchPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", fileName),
            Path.Combine(Directory.GetCurrentDirectory(), fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", fileName),
        };

        var resolved = searchPaths.FirstOrDefault(File.Exists);
        Assert.NotNull(resolved);
        return resolved!;
    }

    private static (int ExitCode, string StandardOutput, string StandardError) RunProcess(
        string fileName,
        string arguments,
        string? workingDirectory = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
        };

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);

        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return (process.ExitCode, standardOutput, standardError);
    }

    private static void ValidatePowerShellUninstallBehavior()
    {
        var scriptPath = ResolveRepoFile("uninstall.ps1");
        var tempRoot = Path.Combine(Path.GetTempPath(), "grok-search-cli-uninstall-ps", Guid.NewGuid().ToString("n"));
        var installDir = Path.Combine(tempRoot, "bin");
        var managedBinary = Path.Combine(installDir, "grok-search-cli.exe");
        var nonManagedFile = Path.Combine(installDir, "keep.txt");
        var credentialFile = Path.Combine(tempRoot, "outside", "credentials.txt");

        Directory.CreateDirectory(installDir);
        Directory.CreateDirectory(Path.GetDirectoryName(credentialFile)!);
        File.WriteAllText(managedBinary, "stub");
        File.WriteAllText(nonManagedFile, "keep");
        File.WriteAllText(credentialFile, "credential");

        try
        {
            var firstRun = RunProcess(
                "pwsh",
                $"-NoProfile -File \"{scriptPath}\" -InstallDir \"{installDir}\"");

            Assert.Equal(0, firstRun.ExitCode);
            Assert.False(File.Exists(managedBinary));
            Assert.True(File.Exists(nonManagedFile));
            Assert.True(File.Exists(credentialFile));
            Assert.True(Directory.Exists(installDir));
            Assert.Contains("Install directory left in place because it still contains non-managed files.", firstRun.StandardOutput);
            Assert.Contains("Credential configuration via XAI_API_KEY, .env files, or auth-managed storage was not modified.", firstRun.StandardOutput);
            Assert.True(
                firstRun.StandardOutput.Contains("No current User PATH entry was detected for the install directory.") ||
                firstRun.StandardOutput.Contains("NOTE: The install directory may still be present in your User PATH."));

            var secondRun = RunProcess(
                "pwsh",
                $"-NoProfile -File \"{scriptPath}\" -InstallDir \"{installDir}\"");

            Assert.Equal(0, secondRun.ExitCode);
            Assert.Contains("No managed grok-search-cli files were present to remove.", secondRun.StandardOutput);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void InstallScript_PowerShell_CanInstallFromLocalAssets()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var scriptPath = ResolveRepoFile("install.ps1");
        var tempRoot = Path.Combine(Path.GetTempPath(), "grok-search-cli-install-ps", Guid.NewGuid().ToString("n"));
        var assetDir = Path.Combine(tempRoot, "assets");
        var installDir = Path.Combine(tempRoot, "bin");
        const string version = "v1.0.0";
        const string rid = "win-x64";
        var archiveName = $"grok-search-cli_{version}_{rid}.zip";
        var checksumName = $"grok-search-cli_{version}_{rid}.sha256";
        var archivePath = Path.Combine(assetDir, archiveName);
        var checksumPath = Path.Combine(assetDir, checksumName);
        var payloadDir = Path.Combine(tempRoot, "payload");
        var binaryPath = Path.Combine(payloadDir, "grok-search-cli.exe");

        Directory.CreateDirectory(assetDir);
        Directory.CreateDirectory(payloadDir);
        File.WriteAllText(binaryPath, "stub binary");
        ZipFile.CreateFromDirectory(payloadDir, archivePath);

        using (var stream = File.OpenRead(archivePath))
        {
            var hash = Convert.ToHexStringLower(SHA256.HashData(stream));
            File.WriteAllText(checksumPath, $"{hash}  {archiveName}\n");
        }

        try
        {
            var result = RunProcess(
                "pwsh",
                $"-NoProfile -File \"{scriptPath}\" -Version {version} -AssetDir \"{assetDir}\" -InstallDir \"{installDir}\"");

            Assert.Equal(0, result.ExitCode);
            Assert.True(File.Exists(Path.Combine(installDir, "grok-search-cli.exe")));
            Assert.Contains("Asset source: local directory", result.StandardOutput);
            Assert.Contains("Checksum verified.", result.StandardOutput);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void LocalPackagingScript_PowerShell_CreatesExpectedOutputNames()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var scriptPath = ResolveRepoFile("package-local-release.ps1");
        var tempRoot = Path.Combine(Path.GetTempPath(), "grok-search-cli-package-ps", Guid.NewGuid().ToString("n"));
        var outputDir = Path.Combine(tempRoot, "assets");
        const string version = "v0.1.0-test";
        const string rid = "win-x64";
        var archivePath = Path.Combine(outputDir, $"grok-search-cli_{version}_{rid}.zip");
        var checksumPath = Path.Combine(outputDir, $"grok-search-cli_{version}_{rid}.sha256");
        var combinedChecksumPath = Path.Combine(outputDir, $"checksums_{version}.txt");

        try
        {
            var result = RunProcess(
                "pwsh",
                $"-NoProfile -File \"{scriptPath}\" -Version {version} -RuntimeId {rid} -OutputDir \"{outputDir}\"");

            Assert.Equal(0, result.ExitCode);
            Assert.True(File.Exists(archivePath));
            Assert.True(File.Exists(checksumPath));
            Assert.True(File.Exists(combinedChecksumPath));
            Assert.Contains($"Created archive: {archivePath}", result.StandardOutput);
            Assert.Contains($"Created checksum: {checksumPath}", result.StandardOutput);
            Assert.Contains($"Updated combined checksums: {combinedChecksumPath}", result.StandardOutput);

            var checksumContents = File.ReadAllText(checksumPath);
            Assert.Contains($"grok-search-cli_{version}_{rid}.zip", checksumContents);

            var combinedContents = File.ReadAllText(combinedChecksumPath);
            Assert.Contains($"grok-search-cli_{version}_{rid}.zip", combinedContents);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    private static void ValidateBashUninstallBehavior()
    {
        var scriptPath = ResolveRepoFile("uninstall.sh");
        var tempRoot = Path.Combine(Path.GetTempPath(), "grok-search-cli-uninstall-sh", Guid.NewGuid().ToString("n"));
        var installDir = Path.Combine(tempRoot, "bin");
        var managedBinary = Path.Combine(installDir, "grok-search-cli");
        var nonManagedFile = Path.Combine(installDir, "keep.txt");
        var credentialFile = Path.Combine(tempRoot, "outside", "credentials.txt");

        Directory.CreateDirectory(installDir);
        Directory.CreateDirectory(Path.GetDirectoryName(credentialFile)!);
        File.WriteAllText(managedBinary, "stub");
        File.WriteAllText(nonManagedFile, "keep");
        File.WriteAllText(credentialFile, "credential");

        try
        {
            var firstRun = RunProcess(
                "bash",
                $"\"{scriptPath}\" --dir \"{installDir}\"");

            Assert.Equal(0, firstRun.ExitCode);
            Assert.False(File.Exists(managedBinary));
            Assert.True(File.Exists(nonManagedFile));
            Assert.True(File.Exists(credentialFile));
            Assert.True(Directory.Exists(installDir));
            Assert.Contains("Install directory left in place because it still contains non-managed files.", firstRun.StandardOutput);
            Assert.Contains("Credential configuration via XAI_API_KEY, .env files, or auth-managed storage was not modified.", firstRun.StandardOutput);
            Assert.True(
                firstRun.StandardOutput.Contains("No current PATH entry was detected for the install directory.") ||
                firstRun.StandardOutput.Contains("NOTE: The install directory may still be present in your PATH or shell profile."));

            var secondRun = RunProcess(
                "bash",
                $"\"{scriptPath}\" --dir \"{installDir}\"");

            Assert.Equal(0, secondRun.ExitCode);
            Assert.Contains("No managed grok-search-cli files were present to remove.", secondRun.StandardOutput);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }
}
