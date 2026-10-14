namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MobilityClaimViewChecks
{
    public static void MobilityClaimViewShowsTheStatusInCapitals()
    {
        var item = MobilityClaim.NewMobilityClaim("code-990", "name-964");
        var view = MobilityClaimView.ToMobilityClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilityClaimView.ToMobilityClaimViews(new List<MobilityClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilityClaimViewShowsTheStatusInCapitals();
    }
}
