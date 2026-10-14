namespace Acme.Workforce.Workplace.Tests.Desk.Adjustment;

using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FallbackDeskLedgerViewChecks
{
    public static void FallbackDeskLedgerViewShowsTheStatusInCapitals()
    {
        var item = FallbackDeskLedger.NewFallbackDeskLedger("code-993", "name-293");
        var view = FallbackDeskLedgerView.ToFallbackDeskLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackDeskLedgerView.ToFallbackDeskLedgerViews(new List<FallbackDeskLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackDeskLedgerViewShowsTheStatusInCapitals();
    }
}
