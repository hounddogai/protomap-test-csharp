namespace Acme.Workforce.Orgdesign.Tests.Headcount.Schedule;

using Acme.Workforce.Orgdesign.Headcount.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class WeightedHeadcountScheduleViewChecks
{
    public static void WeightedHeadcountScheduleViewShowsTheStatusInCapitals()
    {
        var item = WeightedHeadcountSchedule.NewWeightedHeadcountSchedule("code-526", "name-831");
        var view = WeightedHeadcountScheduleView.ToWeightedHeadcountScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedHeadcountScheduleView.ToWeightedHeadcountScheduleViews(new List<WeightedHeadcountSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedHeadcountScheduleViewShowsTheStatusInCapitals();
    }
}
