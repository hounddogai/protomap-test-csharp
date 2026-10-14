namespace Acme.Workforce.Payrollops.Tests.Deduction.Segment;

using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class DeductionSegmentViewChecks
{
    public static void DeductionSegmentViewShowsTheStatusInCapitals()
    {
        var item = DeductionSegment.NewDeductionSegment("code-465", "name-85");
        var view = DeductionSegmentView.ToDeductionSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeductionSegmentView.ToDeductionSegmentViews(new List<DeductionSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeductionSegmentViewShowsTheStatusInCapitals();
    }
}
