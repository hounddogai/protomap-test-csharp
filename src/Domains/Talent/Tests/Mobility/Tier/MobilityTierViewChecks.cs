namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MobilityTierViewChecks
{
    public static void MobilityTierViewShowsTheStatusInCapitals()
    {
        var item = MobilityTier.NewMobilityTier("code-951", "label-341");
        var view = MobilityTierView.ToMobilityTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilityTierView.ToMobilityTierViews(new List<MobilityTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilityTierViewShowsTheStatusInCapitals();
    }
}
