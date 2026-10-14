namespace Acme.Workforce.Talent.Tests.Nomination.Cycle;

using Acme.Workforce.Talent.Nomination.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RollingNominationThresholdViewChecks
{
    public static void RollingNominationThresholdViewShowsTheStatusInCapitals()
    {
        var item = RollingNominationThreshold.NewRollingNominationThreshold("code-765", "title-743");
        var view = RollingNominationThresholdView.ToRollingNominationThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingNominationThresholdView.ToRollingNominationThresholdViews(new List<RollingNominationThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingNominationThresholdViewShowsTheStatusInCapitals();
    }
}
