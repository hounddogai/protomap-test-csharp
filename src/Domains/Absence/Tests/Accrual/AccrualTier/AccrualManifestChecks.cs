namespace Acme.Workforce.Absence.Tests.Accrual.AccrualTier;

using Acme.Workforce.Absence.Accrual.AccrualTier;

/// <summary>Checks of accrual tier code, run by the project's runner.</summary>
public static class AccrualManifestChecks
{
    public static void AccrualManifestStartsInDraft()
    {
        var item = AccrualManifest.NewAccrualManifest("code-730", "label-511");
        Check.Equal(item.Status, AccrualManifestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualManifestMovesToActive()
    {
        var item = AccrualManifest.NewAccrualManifest("code-730", "label-511");
        try
        {
            item.MoveTo(AccrualManifestStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualManifestStatus.Active, "status");
    }

    public static void AccrualManifestRejectsMovesFromClosed()
    {
        var item = AccrualManifest.NewAccrualManifest("code-730", "label-511");
        item.Status = AccrualManifestStatus.Closed;
        Check.Fails(() => item.MoveTo(AccrualManifestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AccrualManifestStartsInDraft();
        AccrualManifestMovesToActive();
        AccrualManifestRejectsMovesFromClosed();
    }
}
