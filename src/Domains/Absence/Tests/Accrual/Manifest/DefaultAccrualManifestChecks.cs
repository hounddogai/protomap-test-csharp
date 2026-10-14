namespace Acme.Workforce.Absence.Tests.Accrual.Manifest;

using Acme.Workforce.Absence.Accrual.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class DefaultAccrualManifestChecks
{
    public static void DefaultAccrualManifestStartsInProposed()
    {
        var item = DefaultAccrualManifest.NewDefaultAccrualManifest("code-120", "title-455");
        Check.Equal(item.Status, DefaultAccrualManifestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultAccrualManifestMovesToRejected()
    {
        var item = DefaultAccrualManifest.NewDefaultAccrualManifest("code-120", "title-455");
        try
        {
            item.MoveTo(DefaultAccrualManifestStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultAccrualManifestStatus.Rejected, "status");
    }

    public static void DefaultAccrualManifestRejectsMovesFromWithdrawn()
    {
        var item = DefaultAccrualManifest.NewDefaultAccrualManifest("code-120", "title-455");
        item.Status = DefaultAccrualManifestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DefaultAccrualManifestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DefaultAccrualManifestStartsInProposed();
        DefaultAccrualManifestMovesToRejected();
        DefaultAccrualManifestRejectsMovesFromWithdrawn();
    }
}
