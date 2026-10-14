namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedVisitorBatchViewChecks
{
    public static void CappedVisitorBatchViewShowsTheStatusInCapitals()
    {
        var item = CappedVisitorBatch.NewCappedVisitorBatch("code-677", "title-142");
        var view = CappedVisitorBatchView.ToCappedVisitorBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedVisitorBatchView.ToCappedVisitorBatchViews(new List<CappedVisitorBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedVisitorBatchViewShowsTheStatusInCapitals();
    }
}
