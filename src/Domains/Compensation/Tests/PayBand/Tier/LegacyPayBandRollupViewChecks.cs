namespace Acme.Workforce.Compensation.Tests.PayBand.Tier;

using Acme.Workforce.Compensation.PayBand.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class LegacyPayBandRollupViewChecks
{
    public static void LegacyPayBandRollupViewShowsTheStatusInCapitals()
    {
        var item = LegacyPayBandRollup.NewLegacyPayBandRollup("code-311", "name-144");
        var view = LegacyPayBandRollupView.ToLegacyPayBandRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyPayBandRollupView.ToLegacyPayBandRollupViews(new List<LegacyPayBandRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyPayBandRollupViewShowsTheStatusInCapitals();
    }
}
