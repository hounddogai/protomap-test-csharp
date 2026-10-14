namespace Acme.Workforce.Engagement.Tests.TownHall.Review;

using Acme.Workforce.Engagement.TownHall.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledTownHallOverrideViewChecks
{
    public static void ScheduledTownHallOverrideViewShowsTheStatusInCapitals()
    {
        var item = ScheduledTownHallOverride.NewScheduledTownHallOverride("code-988", "title-93");
        var view = ScheduledTownHallOverrideView.ToScheduledTownHallOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledTownHallOverrideView.ToScheduledTownHallOverrideViews(new List<ScheduledTownHallOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledTownHallOverrideViewShowsTheStatusInCapitals();
    }
}
