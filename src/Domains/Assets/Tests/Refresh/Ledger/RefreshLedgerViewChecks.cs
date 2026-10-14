namespace Acme.Workforce.Assets.Tests.Refresh.Ledger;

using Acme.Workforce.Assets.Refresh.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RefreshLedgerViewChecks
{
    public static void RefreshLedgerViewShowsTheStatusInCapitals()
    {
        var item = RefreshLedger.NewRefreshLedger("code-999", "label-741");
        var view = RefreshLedgerView.ToRefreshLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RefreshLedgerView.ToRefreshLedgerViews(new List<RefreshLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RefreshLedgerViewShowsTheStatusInCapitals();
    }
}
