namespace Acme.Workforce.Talent.Tests.Mobility.OverrideItem;

using Acme.Workforce.Talent.Mobility.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class WeightedMobilityRollupViewChecks
{
    public static void WeightedMobilityRollupViewShowsTheStatusInCapitals()
    {
        var item = WeightedMobilityRollup.NewWeightedMobilityRollup("code-599", "name-549");
        var view = WeightedMobilityRollupView.ToWeightedMobilityRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedMobilityRollupView.ToWeightedMobilityRollupViews(new List<WeightedMobilityRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedMobilityRollupViewShowsTheStatusInCapitals();
    }
}
