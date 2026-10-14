namespace Acme.Workforce.Assets.Tests.Refresh.Ledger;

using Acme.Workforce.Assets.Refresh.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RefreshLedgerChecks
{
    public static void RefreshLedgerStartsInNew()
    {
        var item = RefreshLedger.NewRefreshLedger("code-999", "label-741");
        Check.Equal(item.Status, RefreshLedgerStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshLedgerMovesToRunning()
    {
        var item = RefreshLedger.NewRefreshLedger("code-999", "label-741");
        try
        {
            item.MoveTo(RefreshLedgerStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshLedgerStatus.Running, "status");
    }

    public static void RefreshLedgerRejectsMovesFromFailed()
    {
        var item = RefreshLedger.NewRefreshLedger("code-999", "label-741");
        item.Status = RefreshLedgerStatus.Failed;
        Check.Fails(() => item.MoveTo(RefreshLedgerStatus.New), "move");
    }

    public static void RunAll()
    {
        RefreshLedgerStartsInNew();
        RefreshLedgerMovesToRunning();
        RefreshLedgerRejectsMovesFromFailed();
    }
}
