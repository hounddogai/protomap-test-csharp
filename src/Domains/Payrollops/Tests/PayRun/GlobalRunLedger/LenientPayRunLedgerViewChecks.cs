namespace Acme.Workforce.Payrollops.Tests.PayRun.GlobalRunLedger;

using Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

/// <summary>Checks of global run ledger code, run by the project's runner.</summary>
public static class LenientPayRunLedgerViewChecks
{
    public static void LenientPayRunLedgerViewShowsTheStatusInCapitals()
    {
        var item = LenientPayRunLedger.NewLenientPayRunLedger("code-975", "name-288");
        var view = LenientPayRunLedgerView.ToLenientPayRunLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientPayRunLedgerView.ToLenientPayRunLedgerViews(new List<LenientPayRunLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientPayRunLedgerViewShowsTheStatusInCapitals();
    }
}
