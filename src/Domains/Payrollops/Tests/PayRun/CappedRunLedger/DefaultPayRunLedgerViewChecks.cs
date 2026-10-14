namespace Acme.Workforce.Payrollops.Tests.PayRun.CappedRunLedger;

using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Checks of capped run ledger code, run by the project's runner.</summary>
public static class DefaultPayRunLedgerViewChecks
{
    public static void DefaultPayRunLedgerViewShowsTheStatusInCapitals()
    {
        var item = DefaultPayRunLedger.NewDefaultPayRunLedger("code-455", "title-272");
        var view = DefaultPayRunLedgerView.ToDefaultPayRunLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultPayRunLedgerView.ToDefaultPayRunLedgerViews(new List<DefaultPayRunLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultPayRunLedgerViewShowsTheStatusInCapitals();
    }
}
