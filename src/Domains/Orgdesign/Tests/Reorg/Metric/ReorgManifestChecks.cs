namespace Acme.Workforce.Orgdesign.Tests.Reorg.Metric;

using Acme.Workforce.Orgdesign.Reorg.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReorgManifestChecks
{
    public static void ReorgManifestStartsInIdle()
    {
        var item = ReorgManifest.NewReorgManifest("code-311", "title-945");
        Check.Equal(item.Status, ReorgManifestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgManifestMovesToWarming()
    {
        var item = ReorgManifest.NewReorgManifest("code-311", "title-945");
        try
        {
            item.MoveTo(ReorgManifestStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgManifestStatus.Warming, "status");
    }

    public static void ReorgManifestRejectsMovesFromStopped()
    {
        var item = ReorgManifest.NewReorgManifest("code-311", "title-945");
        item.Status = ReorgManifestStatus.Stopped;
        Check.Fails(() => item.MoveTo(ReorgManifestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ReorgManifestStartsInIdle();
        ReorgManifestMovesToWarming();
        ReorgManifestRejectsMovesFromStopped();
    }
}
