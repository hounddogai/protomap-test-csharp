namespace Acme.Workforce.Absence.Tests.Accrual.Digest;

using Acme.Workforce.Absence.Accrual.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class TieredAccrualLedgerChecks
{
    public static void TieredAccrualLedgerStartsInNew()
    {
        var item = TieredAccrualLedger.NewTieredAccrualLedger("code-274", "name-321");
        Check.Equal(item.Status, TieredAccrualLedgerStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredAccrualLedgerMovesToQueued()
    {
        var item = TieredAccrualLedger.NewTieredAccrualLedger("code-274", "name-321");
        try
        {
            item.MoveTo(TieredAccrualLedgerStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredAccrualLedgerStatus.Queued, "status");
    }

    public static void TieredAccrualLedgerRejectsMovesFromFailed()
    {
        var item = TieredAccrualLedger.NewTieredAccrualLedger("code-274", "name-321");
        item.Status = TieredAccrualLedgerStatus.Failed;
        Check.Fails(() => item.MoveTo(TieredAccrualLedgerStatus.New), "move");
    }

    public static void RunAll()
    {
        TieredAccrualLedgerStartsInNew();
        TieredAccrualLedgerMovesToQueued();
        TieredAccrualLedgerRejectsMovesFromFailed();
    }
}
