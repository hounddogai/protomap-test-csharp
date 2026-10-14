namespace Acme.Workforce.Engagement.Tests.TownHall.Adjustment;

using Acme.Workforce.Engagement.TownHall.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingTownHallThresholdViewChecks
{
    public static void RollingTownHallThresholdViewShowsTheStatusInCapitals()
    {
        var item = RollingTownHallThreshold.NewRollingTownHallThreshold("code-441", "title-155");
        var view = RollingTownHallThresholdView.ToRollingTownHallThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingTownHallThresholdView.ToRollingTownHallThresholdViews(new List<RollingTownHallThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingTownHallThresholdViewShowsTheStatusInCapitals();
    }
}
