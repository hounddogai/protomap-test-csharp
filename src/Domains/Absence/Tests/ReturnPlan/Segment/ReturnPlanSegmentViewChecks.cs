namespace Acme.Workforce.Absence.Tests.ReturnPlan.Segment;

using Acme.Workforce.Absence.ReturnPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ReturnPlanSegmentViewChecks
{
    public static void ReturnPlanSegmentViewShowsTheStatusInCapitals()
    {
        var item = ReturnPlanSegment.NewReturnPlanSegment("code-965", "name-620");
        var view = ReturnPlanSegmentView.ToReturnPlanSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReturnPlanSegmentView.ToReturnPlanSegmentViews(new List<ReturnPlanSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReturnPlanSegmentViewShowsTheStatusInCapitals();
    }
}
