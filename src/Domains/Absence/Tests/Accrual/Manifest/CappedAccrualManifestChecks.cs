namespace Acme.Workforce.Absence.Tests.Accrual.Manifest;

using Acme.Workforce.Absence.Accrual.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class CappedAccrualManifestChecks
{
    public static void CappedAccrualManifestStartsInNew()
    {
        var item = CappedAccrualManifest.NewCappedAccrualManifest("code-509", "label-732");
        Check.Equal(item.Status, CappedAccrualManifestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedAccrualManifestMovesToRunning()
    {
        var item = CappedAccrualManifest.NewCappedAccrualManifest("code-509", "label-732");
        try
        {
            item.MoveTo(CappedAccrualManifestStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedAccrualManifestStatus.Running, "status");
    }

    public static void CappedAccrualManifestRejectsMovesFromFailed()
    {
        var item = CappedAccrualManifest.NewCappedAccrualManifest("code-509", "label-732");
        item.Status = CappedAccrualManifestStatus.Failed;
        Check.Fails(() => item.MoveTo(CappedAccrualManifestStatus.New), "move");
    }

    public static void RunAll()
    {
        CappedAccrualManifestStartsInNew();
        CappedAccrualManifestMovesToRunning();
        CappedAccrualManifestRejectsMovesFromFailed();
    }
}
