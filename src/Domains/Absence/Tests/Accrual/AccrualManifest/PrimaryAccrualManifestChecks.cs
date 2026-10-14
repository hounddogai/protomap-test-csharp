namespace Acme.Workforce.Absence.Tests.Accrual.AccrualManifest;

using Acme.Workforce.Absence.Accrual.AccrualManifest;

/// <summary>Checks of accrual manifest code, run by the project's runner.</summary>
public static class PrimaryAccrualManifestChecks
{
    public static void PrimaryAccrualManifestStartsInPlanned()
    {
        var item = PrimaryAccrualManifest.NewPrimaryAccrualManifest("code-572", "title-983");
        Check.Equal(item.Status, PrimaryAccrualManifestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryAccrualManifestMovesToCancelled()
    {
        var item = PrimaryAccrualManifest.NewPrimaryAccrualManifest("code-572", "title-983");
        try
        {
            item.MoveTo(PrimaryAccrualManifestStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryAccrualManifestStatus.Cancelled, "status");
    }

    public static void PrimaryAccrualManifestRejectsMovesFromCancelled()
    {
        var item = PrimaryAccrualManifest.NewPrimaryAccrualManifest("code-572", "title-983");
        item.Status = PrimaryAccrualManifestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PrimaryAccrualManifestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PrimaryAccrualManifestStartsInPlanned();
        PrimaryAccrualManifestMovesToCancelled();
        PrimaryAccrualManifestRejectsMovesFromCancelled();
    }
}
