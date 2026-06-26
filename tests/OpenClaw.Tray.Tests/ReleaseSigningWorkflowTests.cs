namespace OpenClaw.Tray.Tests;

public sealed class ReleaseSigningWorkflowTests
{
    [Fact]
    public void UpstreamReleaseJob_IsDisabledForUnsignedQVerisBotMilestone()
    {
        var workflow = File.ReadAllText(Path.Combine(GetRepositoryRoot(), ".github", "workflows", "ci.yml"));

        Assert.Contains(
            "if: false # QVerisBot first-stage releases use unsigned-release.yml; upstream signing is intentionally disabled.",
            workflow);
    }

    [Fact]
    public void UnsignedReleaseWorkflow_DoesNotRequireSigningSecrets()
    {
        var workflow = File.ReadAllText(Path.Combine(
            GetRepositoryRoot(), ".github", "workflows", "unsigned-release.yml"));

        Assert.DoesNotContain("azure/login", workflow);
        Assert.DoesNotContain("azure/artifact-signing-action", workflow);
        Assert.DoesNotContain("AZURE_", workflow);
        Assert.DoesNotContain("environment: release-signing", workflow);
        Assert.DoesNotContain("Test-ReleaseExecutableSignatures.ps1", workflow);
    }

    [Fact]
    public void UnsignedReleaseWorkflow_BuildsExpectedQVerisBotInstallers()
    {
        var workflow = File.ReadAllText(Path.Combine(
            GetRepositoryRoot(), ".github", "workflows", "unsigned-release.yml"));

        Assert.Contains("workflow_dispatch:", workflow);
        Assert.Contains("push:", workflow);
        Assert.Contains("- 'v*'", workflow);
        Assert.Contains("rid: win-x64", workflow);
        Assert.Contains("rid: win-arm64", workflow);
        Assert.Contains("dotnet-version: 10.0.x", workflow);
        Assert.Contains("Publish SetupEngine.UI", workflow);
        Assert.Contains("dotnet publish src/OpenClaw.SetupEngine.UI", workflow);
        Assert.Contains(@"Copy-Item publish-setup\* publish\SetupEngine\ -Recurse -Force", workflow);
        Assert.Contains("Build QVerisBot installer", workflow);
        Assert.Contains("Output/QVerisBot-Setup-${{ matrix.arch }}.exe", workflow);
        Assert.Contains("QVerisBot-Setup-x64.exe", workflow);
        Assert.Contains("QVerisBot-Setup-arm64.exe", workflow);
        Assert.Contains("QVerisBot-SHA256SUMS.txt", workflow);
        Assert.Contains("prerelease: true", workflow);
        Assert.Contains("make_latest: false", workflow);
        Assert.Contains("openclaw://", workflow);
        Assert.Contains("qverisbot", workflow);
        Assert.Contains("openclaw", workflow);
    }

    [Fact]
    public void UnsignedReleaseWorkflow_BundlesAndVerifiesNativeRuntimeDependencies()
    {
        var root = GetRepositoryRoot();
        var workflow = File.ReadAllText(Path.Combine(root, ".github", "workflows", "unsigned-release.yml"));
        var installer = File.ReadAllText(Path.Combine(root, "installer.iss"));
        var verifier = File.ReadAllText(Path.Combine(root, "scripts", "Test-ReleaseNativeDependencies.ps1"));
        var targets = File.ReadAllText(Path.Combine(root, "src", "Directory.Build.targets"));

        Assert.Contains("Test-ReleaseNativeDependencies.ps1 @args", workflow);
        Assert.Contains("Test-ReleaseNativeDependencies.ps1 @verifyArgs", workflow);
        Assert.Contains("-RequireAppLocalVCRuntime", workflow);
        Assert.Contains("-RequireInstallerVCRedist", workflow);
        Assert.Contains("-InstallerVCRedistPath", workflow);
        Assert.Contains("https://aka.ms/vc14/vc_redist.x64.exe", workflow);
        Assert.Contains("https://aka.ms/vc14/vc_redist.arm64.exe", workflow);
        Assert.Contains("Get-AuthenticodeSignature -LiteralPath", workflow);
        Assert.Contains("O=Microsoft Corporation", workflow);
        Assert.Contains("/DvcRedist=${{ matrix.vc_redist_name }}", workflow);
        Assert.DoesNotContain("copy vc_redist.x64.exe publish-x64", workflow);
        Assert.DoesNotContain("copy vc_redist.x64.exe publish-arm64", workflow);
        Assert.Contains("AfterInstall: InstallVCRuntime", installer);
        Assert.Contains("Exec(", installer);
        Assert.Contains("ResultCode = 3010", installer);
        Assert.Contains("ShouldLaunchTray", installer);
        Assert.Contains("Skipping post-install tray launch", installer);
        Assert.DoesNotContain(@"Filename: ""{tmp}\vc_redist.exe""", installer);
        Assert.Contains("Get-AuthenticodeSignature -LiteralPath $File.FullName", verifier);
        Assert.Contains("Get-VCRuntimeFiles", verifier);
        Assert.Contains("vcruntime140.dll", verifier);
        Assert.Contains("libsodium.dll", verifier);
        Assert.Contains("OpenClawNativeDependencyProbe", verifier);
        Assert.Contains("SkipNativeLoadProbe", verifier);
        Assert.Contains("CopyOpenClawVCRuntimeToPublish", targets);
        Assert.Contains("ResolveOpenClawVCRuntimeArm64FromVSInstall", targets);
    }

    [Fact]
    public void CiWorkflow_PausesMsixForFirstStage()
    {
        var workflow = File.ReadAllText(Path.Combine(GetRepositoryRoot(), ".github", "workflows", "ci.yml"));

        Assert.Contains("if: false # Paused for alpha.4; ship Inno setup and portable ZIP artifacts only.", workflow);
        Assert.Contains("needs: [repo-hygiene, test, e2etests, build]", workflow);
        Assert.DoesNotContain("Download win-x64 MSIX artifact", workflow);
        Assert.DoesNotContain("Download win-arm64 MSIX artifact", workflow);
        Assert.DoesNotContain("Sign Release MSIX Packages", workflow);
    }

    private static string GetRepositoryRoot()
    {
        var env = Environment.GetEnvironmentVariable("OPENCLAW_REPO_ROOT");
        if (!string.IsNullOrWhiteSpace(env) && Directory.Exists(env))
            return env;

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "openclaw-windows-node.slnx")) &&
                Directory.Exists(Path.Combine(directory.FullName, "src")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find repository root.");
    }
}
