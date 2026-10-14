namespace Acme.Workforce.Workplace.Tests.Visitor.Schedule;

using Acme.Workforce.Workplace.Visitor.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class VisitorScheduleViewChecks
{
    public static void VisitorScheduleViewShowsTheStatusInCapitals()
    {
        var item = VisitorSchedule.NewVisitorSchedule("code-38", "label-514");
        var view = VisitorScheduleView.ToVisitorScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorScheduleView.ToVisitorScheduleViews(new List<VisitorSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorScheduleViewShowsTheStatusInCapitals();
    }
}
