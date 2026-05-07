using XaiSearchCli;
using XaiSearchCli.Configuration;

namespace XaiSearchCli.Tests;

public class AuthFlowTests
{
    [Fact]
    public void CredentialStore_WriteAndRead()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var previousXdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        try
        {
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", tempDir);

            CredentialStore.Write("test-api-key");
            Assert.True(CredentialStore.Exists());
            Assert.Equal("test-api-key", CredentialStore.Read());

            var expectedPath = Path.Combine(tempDir, "grok-search-cli", "credentials.env");
            Assert.Equal(expectedPath, CredentialStore.GetStorePath());
            Assert.True(File.Exists(expectedPath));
        }
        finally
        {
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", previousXdg);
            TryDeleteDirectory(tempDir);
        }
    }

    [Fact]
    public void CredentialStore_Delete()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var previousXdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        try
        {
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", tempDir);
            CredentialStore.Write("test-api-key");

            Assert.True(CredentialStore.Exists());

            CredentialStore.Delete();

            Assert.False(CredentialStore.Exists());
            Assert.Null(CredentialStore.Read());
        }
        finally
        {
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", previousXdg);
            TryDeleteDirectory(tempDir);
        }
    }

    [Fact]
    public void CredentialStore_ReadNonexistentReturnsNull()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var previousXdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        try
        {
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", tempDir);

            Assert.Null(CredentialStore.Read());
            Assert.False(CredentialStore.Exists());
        }
        finally
        {
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", previousXdg);
            TryDeleteDirectory(tempDir);
        }
    }

    [Fact]
    public void CredentialResolution_EnvVarOverridesDotEnvAndStore()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var previousXdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        var previousKey = Environment.GetEnvironmentVariable("XAI_API_KEY");
        var previousCwd = Directory.GetCurrentDirectory();
        try
        {
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", tempDir);
            CredentialStore.Write("store-key");

            File.WriteAllText(Path.Combine(tempDir, ".env"), "XAI_API_KEY=dotenv-key");
            Directory.SetCurrentDirectory(tempDir);

            Environment.SetEnvironmentVariable("XAI_API_KEY", "env-key");

            var stderr = new StringWriter();
            var result = Credentials.Resolve(stderr);
            Assert.Equal("env-key", result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", previousXdg);
            Environment.SetEnvironmentVariable("XAI_API_KEY", previousKey);
            Directory.SetCurrentDirectory(previousCwd);
            TryDeleteDirectory(tempDir);
        }
    }

    [Fact]
    public void CredentialResolution_DotEnvOverridesStore()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var previousXdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        var previousKey = Environment.GetEnvironmentVariable("XAI_API_KEY");
        var previousCwd = Directory.GetCurrentDirectory();
        try
        {
            Environment.SetEnvironmentVariable("XAI_API_KEY", null);
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", tempDir);
            CredentialStore.Write("store-key");

            File.WriteAllText(Path.Combine(tempDir, ".env"), "XAI_API_KEY=dotenv-key");
            Directory.SetCurrentDirectory(tempDir);

            var stderr = new StringWriter();
            var result = Credentials.Resolve(stderr);
            Assert.Equal("dotenv-key", result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", previousXdg);
            Environment.SetEnvironmentVariable("XAI_API_KEY", previousKey);
            Directory.SetCurrentDirectory(previousCwd);
            TryDeleteDirectory(tempDir);
        }
    }

    [Fact]
    public void CredentialResolution_StoreFallback()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var previousXdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        var previousKey = Environment.GetEnvironmentVariable("XAI_API_KEY");
        var previousCwd = Directory.GetCurrentDirectory();
        try
        {
            Environment.SetEnvironmentVariable("XAI_API_KEY", null);
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", tempDir);
            CredentialStore.Write("store-key");

            Directory.SetCurrentDirectory(tempDir);

            var stderr = new StringWriter();
            var result = Credentials.Resolve(stderr);
            Assert.Equal("store-key", result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", previousXdg);
            Environment.SetEnvironmentVariable("XAI_API_KEY", previousKey);
            Directory.SetCurrentDirectory(previousCwd);
            TryDeleteDirectory(tempDir);
        }
    }

    // Note: The error path (no credential source available) calls Environment.Exit,
    // which terminates the test host process. That path is verified by inspecting
    // the source code of Credentials.Resolve and does not have an automated test.

    [Fact]
    public void AuthStatus_ReportsNotConfigured()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var previousXdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        var previousKey = Environment.GetEnvironmentVariable("XAI_API_KEY");
        var previousCwd = Directory.GetCurrentDirectory();
        try
        {
            Environment.SetEnvironmentVariable("XAI_API_KEY", null);
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", tempDir);
            Directory.SetCurrentDirectory(tempDir);

            var stderr = new StringWriter();
            AuthCommand.Handle(["status"], stderr);
            var output = stderr.ToString();

            Assert.Contains("not configured", output);
        }
        finally
        {
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", previousXdg);
            Environment.SetEnvironmentVariable("XAI_API_KEY", previousKey);
            Directory.SetCurrentDirectory(previousCwd);
            TryDeleteDirectory(tempDir);
        }
    }

    [Fact]
    public void AuthStatus_ReportsManagedStore()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var previousXdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        var previousKey = Environment.GetEnvironmentVariable("XAI_API_KEY");
        var previousCwd = Directory.GetCurrentDirectory();
        try
        {
            Environment.SetEnvironmentVariable("XAI_API_KEY", null);
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", tempDir);
            CredentialStore.Write("store-key");
            Directory.SetCurrentDirectory(tempDir);

            var stderr = new StringWriter();
            AuthCommand.Handle(["status"], stderr);
            var output = stderr.ToString();

            Assert.Contains("managed credential store", output);
        }
        finally
        {
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", previousXdg);
            Environment.SetEnvironmentVariable("XAI_API_KEY", previousKey);
            Directory.SetCurrentDirectory(previousCwd);
            TryDeleteDirectory(tempDir);
        }
    }

    [Fact]
    public void AuthStatus_ReportsEnvVar()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var previousKey = Environment.GetEnvironmentVariable("XAI_API_KEY");
        var previousCwd = Directory.GetCurrentDirectory();
        try
        {
            Environment.SetEnvironmentVariable("XAI_API_KEY", "env-override");
            Directory.SetCurrentDirectory(tempDir);

            var stderr = new StringWriter();
            AuthCommand.Handle(["status"], stderr);
            var output = stderr.ToString();

            Assert.Contains("environment variable", output);
            Assert.DoesNotContain("env-override", output);
        }
        finally
        {
            Environment.SetEnvironmentVariable("XAI_API_KEY", previousKey);
            Directory.SetCurrentDirectory(previousCwd);
            TryDeleteDirectory(tempDir);
        }
    }

    [Fact]
    public void AuthLogout_RemovesManagedCredentials()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var previousXdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        var previousKey = Environment.GetEnvironmentVariable("XAI_API_KEY");
        try
        {
            Environment.SetEnvironmentVariable("XAI_API_KEY", null);
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", tempDir);
            CredentialStore.Write("some-key");

            Assert.True(CredentialStore.Exists());

            var stderr = new StringWriter();
            AuthCommand.Handle(["logout"], stderr);
            var output = stderr.ToString();

            Assert.False(CredentialStore.Exists());
            Assert.Contains("cleared", output);
        }
        finally
        {
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", previousXdg);
            Environment.SetEnvironmentVariable("XAI_API_KEY", previousKey);
            TryDeleteDirectory(tempDir);
        }
    }

    [Fact]
    public void AuthLogout_NoopWhenNoCredentials()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var previousXdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        try
        {
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", tempDir);

            var stderr = new StringWriter();
            AuthCommand.Handle(["logout"], stderr);
            var output = stderr.ToString();

            Assert.Contains("no managed credentials found", output);
        }
        finally
        {
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", previousXdg);
            TryDeleteDirectory(tempDir);
        }
    }

    // Note: The auth command error paths (unknown subcommand, missing subcommand)
    // call Environment.Exit, which terminates the test host process. Those paths
    // are verified by inspecting the source code of AuthCommand.Handle and do not
    // have automated tests.

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
        catch
        {
            // Best-effort cleanup
        }
    }
}
