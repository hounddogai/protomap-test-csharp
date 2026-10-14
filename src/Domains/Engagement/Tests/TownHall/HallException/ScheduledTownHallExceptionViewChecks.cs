namespace Acme.Workforce.Engagement.Tests.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>Checks of hall exception code, run by the project's runner.</summary>
public static class ScheduledTownHallExceptionViewChecks
{
    public static void ScheduledTownHallExceptionViewShowsTheStatusInCapitals()
    {
        var item = ScheduledTownHallException.NewScheduledTownHallException("code-218", "name-6");
        var view = ScheduledTownHallExceptionView.ToScheduledTownHallExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledTownHallExceptionView.ToScheduledTownHallExceptionViews(new List<ScheduledTownHallException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledTownHallExceptionViewShowsTheStatusInCapitals();
    }
}
