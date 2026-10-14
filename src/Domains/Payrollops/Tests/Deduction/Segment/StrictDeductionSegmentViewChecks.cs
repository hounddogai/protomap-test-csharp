namespace Acme.Workforce.Payrollops.Tests.Deduction.Segment;

using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StrictDeductionSegmentViewChecks
{
    public static void StrictDeductionSegmentViewShowsTheStatusInCapitals()
    {
        var item = StrictDeductionSegment.NewStrictDeductionSegment("code-200", "name-78");
        var view = StrictDeductionSegmentView.ToStrictDeductionSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictDeductionSegmentView.ToStrictDeductionSegmentViews(new List<StrictDeductionSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictDeductionSegmentViewShowsTheStatusInCapitals();
    }
}
