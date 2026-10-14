namespace Acme.Workforce.Talent.Tests.Mobility.Preference;

using Acme.Workforce.Talent.Mobility.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DefaultMobilityClaimViewChecks
{
    public static void DefaultMobilityClaimViewShowsTheStatusInCapitals()
    {
        var item = DefaultMobilityClaim.NewDefaultMobilityClaim("code-628", "name-29");
        var view = DefaultMobilityClaimView.ToDefaultMobilityClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultMobilityClaimView.ToDefaultMobilityClaimViews(new List<DefaultMobilityClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultMobilityClaimViewShowsTheStatusInCapitals();
    }
}
