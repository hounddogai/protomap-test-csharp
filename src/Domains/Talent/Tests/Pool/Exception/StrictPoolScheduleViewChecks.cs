namespace Acme.Workforce.Talent.Tests.Pool.Exception;

using Acme.Workforce.Talent.Pool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class StrictPoolScheduleViewChecks
{
    public static void StrictPoolScheduleViewShowsTheStatusInCapitals()
    {
        var item = StrictPoolSchedule.NewStrictPoolSchedule("code-70", "name-626");
        var view = StrictPoolScheduleView.ToStrictPoolScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictPoolScheduleView.ToStrictPoolScheduleViews(new List<StrictPoolSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictPoolScheduleViewShowsTheStatusInCapitals();
    }
}
