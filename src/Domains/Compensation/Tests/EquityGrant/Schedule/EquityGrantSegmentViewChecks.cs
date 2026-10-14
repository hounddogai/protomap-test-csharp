namespace Acme.Workforce.Compensation.Tests.EquityGrant.Schedule;

using Acme.Workforce.Compensation.EquityGrant.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class EquityGrantSegmentViewChecks
{
    public static void EquityGrantSegmentViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantSegment.NewEquityGrantSegment("code-966", "name-313");
        var view = EquityGrantSegmentView.ToEquityGrantSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantSegmentView.ToEquityGrantSegmentViews(new List<EquityGrantSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantSegmentViewShowsTheStatusInCapitals();
    }
}
