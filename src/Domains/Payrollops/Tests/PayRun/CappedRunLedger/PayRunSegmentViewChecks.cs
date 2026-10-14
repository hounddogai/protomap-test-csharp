namespace Acme.Workforce.Payrollops.Tests.PayRun.CappedRunLedger;

using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Checks of capped run ledger code, run by the project's runner.</summary>
public static class PayRunSegmentViewChecks
{
    public static void PayRunSegmentViewShowsTheStatusInCapitals()
    {
        var item = PayRunSegment.NewPayRunSegment("code-355", "label-230");
        var view = PayRunSegmentView.ToPayRunSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayRunSegmentView.ToPayRunSegmentViews(new List<PayRunSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayRunSegmentViewShowsTheStatusInCapitals();
    }
}
