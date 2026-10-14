namespace Acme.Workforce.Orgdesign.Tests.Headcount.Tier;

using Acme.Workforce.Orgdesign.Headcount.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HeadcountScheduleViewChecks
{
    public static void HeadcountScheduleViewShowsTheStatusInCapitals()
    {
        var item = HeadcountSchedule.NewHeadcountSchedule("code-745", "label-489");
        var view = HeadcountScheduleView.ToHeadcountScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HeadcountScheduleView.ToHeadcountScheduleViews(new List<HeadcountSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HeadcountScheduleViewShowsTheStatusInCapitals();
    }
}
