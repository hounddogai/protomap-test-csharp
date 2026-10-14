namespace Acme.Workforce.Compliance.Tests.Exception.Profile;

using Acme.Workforce.Compliance.Exception.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PendingExceptionManifestChecks
{
    public static void PendingExceptionManifestStartsInOpen()
    {
        var item = PendingExceptionManifest.NewPendingExceptionManifest("code-546", "label-299");
        Check.Equal(item.Status, PendingExceptionManifestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingExceptionManifestMovesToReopened()
    {
        var item = PendingExceptionManifest.NewPendingExceptionManifest("code-546", "label-299");
        try
        {
            item.MoveTo(PendingExceptionManifestStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingExceptionManifestStatus.Reopened, "status");
    }

    public static void PendingExceptionManifestRejectsMovesFromArchived()
    {
        var item = PendingExceptionManifest.NewPendingExceptionManifest("code-546", "label-299");
        item.Status = PendingExceptionManifestStatus.Archived;
        Check.Fails(() => item.MoveTo(PendingExceptionManifestStatus.Open), "move");
    }

    public static void RunAll()
    {
        PendingExceptionManifestStartsInOpen();
        PendingExceptionManifestMovesToReopened();
        PendingExceptionManifestRejectsMovesFromArchived();
    }
}
