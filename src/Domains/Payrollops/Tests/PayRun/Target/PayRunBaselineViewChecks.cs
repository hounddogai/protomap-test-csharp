namespace Acme.Workforce.Payrollops.Tests.PayRun.Target;

using Acme.Workforce.Payrollops.PayRun.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PayRunBaselineViewChecks
{
    public static void PayRunBaselineViewShowsTheStatusInCapitals()
    {
        var item = PayRunBaseline.NewPayRunBaseline("code-860", "title-972");
        var view = PayRunBaselineView.ToPayRunBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayRunBaselineView.ToPayRunBaselineViews(new List<PayRunBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayRunBaselineViewShowsTheStatusInCapitals();
    }
}
