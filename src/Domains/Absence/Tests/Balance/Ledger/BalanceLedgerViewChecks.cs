namespace Acme.Workforce.Absence.Tests.Balance.Ledger;

using Acme.Workforce.Absence.Balance.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BalanceLedgerViewChecks
{
    public static void BalanceLedgerViewShowsTheStatusInCapitals()
    {
        var item = BalanceLedger.NewBalanceLedger("code-500", "label-216");
        var view = BalanceLedgerView.ToBalanceLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalanceLedgerView.ToBalanceLedgerViews(new List<BalanceLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalanceLedgerViewShowsTheStatusInCapitals();
    }
}
