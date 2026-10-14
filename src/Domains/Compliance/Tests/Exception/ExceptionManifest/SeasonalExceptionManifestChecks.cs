namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionManifest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>Checks of exception manifest code, run by the project's runner.</summary>
public static class SeasonalExceptionManifestChecks
{
    public static void SeasonalExceptionManifestStartsInNew()
    {
        var item = SeasonalExceptionManifest.NewSeasonalExceptionManifest("code-35", "label-841");
        Check.Equal(item.Status, SeasonalExceptionManifestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalExceptionManifestMovesToRunning()
    {
        var item = SeasonalExceptionManifest.NewSeasonalExceptionManifest("code-35", "label-841");
        try
        {
            item.MoveTo(SeasonalExceptionManifestStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalExceptionManifestStatus.Running, "status");
    }

    public static void SeasonalExceptionManifestRejectsMovesFromFailed()
    {
        var item = SeasonalExceptionManifest.NewSeasonalExceptionManifest("code-35", "label-841");
        item.Status = SeasonalExceptionManifestStatus.Failed;
        Check.Fails(() => item.MoveTo(SeasonalExceptionManifestStatus.New), "move");
    }

    public static void RunAll()
    {
        SeasonalExceptionManifestStartsInNew();
        SeasonalExceptionManifestMovesToRunning();
        SeasonalExceptionManifestRejectsMovesFromFailed();
    }
}
