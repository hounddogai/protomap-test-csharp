namespace Acme.Workforce.Payrollops.Tests.Deduction.Segment;

using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class DefaultDeductionSegmentViewChecks
{
    public static void DefaultDeductionSegmentViewShowsTheStatusInCapitals()
    {
        var item = DefaultDeductionSegment.NewDefaultDeductionSegment("code-175", "label-175");
        var view = DefaultDeductionSegmentView.ToDefaultDeductionSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultDeductionSegmentView.ToDefaultDeductionSegmentViews(new List<DefaultDeductionSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultDeductionSegmentViewShowsTheStatusInCapitals();
    }
}
