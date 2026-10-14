namespace Acme.Workforce.Absence.Tests.Accrual.Segment;

using Acme.Workforce.Absence.Accrual.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AccrualSegmentViewChecks
{
    public static void AccrualSegmentViewShowsTheStatusInCapitals()
    {
        var item = AccrualSegment.NewAccrualSegment("code-884", "name-632");
        var view = AccrualSegmentView.ToAccrualSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualSegmentView.ToAccrualSegmentViews(new List<AccrualSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualSegmentViewShowsTheStatusInCapitals();
    }
}
