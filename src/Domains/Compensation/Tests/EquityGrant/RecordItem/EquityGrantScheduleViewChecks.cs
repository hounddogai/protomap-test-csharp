namespace Acme.Workforce.Compensation.Tests.EquityGrant.RecordItem;

using Acme.Workforce.Compensation.EquityGrant.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class EquityGrantScheduleViewChecks
{
    public static void EquityGrantScheduleViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantSchedule.NewEquityGrantSchedule("code-536", "title-180");
        var view = EquityGrantScheduleView.ToEquityGrantScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantScheduleView.ToEquityGrantScheduleViews(new List<EquityGrantSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantScheduleViewShowsTheStatusInCapitals();
    }
}
