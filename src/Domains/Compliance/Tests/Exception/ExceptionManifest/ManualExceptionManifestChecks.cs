namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionManifest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>Checks of exception manifest code, run by the project's runner.</summary>
public static class ManualExceptionManifestChecks
{
    public static void ManualExceptionManifestStartsInIdle()
    {
        var item = ManualExceptionManifest.NewManualExceptionManifest("code-741", "name-390");
        Check.Equal(item.Status, ManualExceptionManifestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualExceptionManifestMovesToWarming()
    {
        var item = ManualExceptionManifest.NewManualExceptionManifest("code-741", "name-390");
        try
        {
            item.MoveTo(ManualExceptionManifestStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualExceptionManifestStatus.Warming, "status");
    }

    public static void ManualExceptionManifestRejectsMovesFromStopped()
    {
        var item = ManualExceptionManifest.NewManualExceptionManifest("code-741", "name-390");
        item.Status = ManualExceptionManifestStatus.Stopped;
        Check.Fails(() => item.MoveTo(ManualExceptionManifestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ManualExceptionManifestStartsInIdle();
        ManualExceptionManifestMovesToWarming();
        ManualExceptionManifestRejectsMovesFromStopped();
    }
}
