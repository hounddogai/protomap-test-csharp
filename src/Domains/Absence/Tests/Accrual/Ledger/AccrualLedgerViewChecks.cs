namespace Acme.Workforce.Absence.Tests.Accrual.Ledger;

using Acme.Workforce.Absence.Accrual.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AccrualLedgerViewChecks
{
    public static void AccrualLedgerViewShowsTheStatusInCapitals()
    {
        var item = AccrualLedger.NewAccrualLedger("code-636", "label-913");
        var view = AccrualLedgerView.ToAccrualLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualLedgerView.ToAccrualLedgerViews(new List<AccrualLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualLedgerViewShowsTheStatusInCapitals();
    }
}
