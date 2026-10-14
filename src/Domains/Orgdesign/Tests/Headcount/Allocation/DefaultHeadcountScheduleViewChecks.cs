namespace Acme.Workforce.Orgdesign.Tests.Headcount.Allocation;

using Acme.Workforce.Orgdesign.Headcount.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class DefaultHeadcountScheduleViewChecks
{
    public static void DefaultHeadcountScheduleViewShowsTheStatusInCapitals()
    {
        var item = DefaultHeadcountSchedule.NewDefaultHeadcountSchedule("code-81", "title-324");
        var view = DefaultHeadcountScheduleView.ToDefaultHeadcountScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultHeadcountScheduleView.ToDefaultHeadcountScheduleViews(new List<DefaultHeadcountSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultHeadcountScheduleViewShowsTheStatusInCapitals();
    }
}
