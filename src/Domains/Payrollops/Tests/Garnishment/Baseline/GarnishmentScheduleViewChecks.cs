namespace Acme.Workforce.Payrollops.Tests.Garnishment.Baseline;

using Acme.Workforce.Payrollops.Garnishment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class GarnishmentScheduleViewChecks
{
    public static void GarnishmentScheduleViewShowsTheStatusInCapitals()
    {
        var item = GarnishmentSchedule.NewGarnishmentSchedule("code-546", "title-269");
        var view = GarnishmentScheduleView.ToGarnishmentScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GarnishmentScheduleView.ToGarnishmentScheduleViews(new List<GarnishmentSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GarnishmentScheduleViewShowsTheStatusInCapitals();
    }
}
