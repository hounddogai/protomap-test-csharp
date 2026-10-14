namespace Acme.Workforce.Absence.Tests.Holiday.Metric;

using Acme.Workforce.Absence.Holiday.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class DefaultHolidayMilestoneViewChecks
{
    public static void DefaultHolidayMilestoneViewShowsTheStatusInCapitals()
    {
        var item = DefaultHolidayMilestone.NewDefaultHolidayMilestone("code-588", "label-452");
        var view = DefaultHolidayMilestoneView.ToDefaultHolidayMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultHolidayMilestoneView.ToDefaultHolidayMilestoneViews(new List<DefaultHolidayMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultHolidayMilestoneViewShowsTheStatusInCapitals();
    }
}
