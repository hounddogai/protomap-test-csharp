namespace Acme.Workforce.Talent.Tests.Mobility.Review;

using Acme.Workforce.Talent.Mobility.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledMobilityRollupViewChecks
{
    public static void ScheduledMobilityRollupViewShowsTheStatusInCapitals()
    {
        var item = ScheduledMobilityRollup.NewScheduledMobilityRollup("code-650", "title-287");
        var view = ScheduledMobilityRollupView.ToScheduledMobilityRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledMobilityRollupView.ToScheduledMobilityRollupViews(new List<ScheduledMobilityRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledMobilityRollupViewShowsTheStatusInCapitals();
    }
}
