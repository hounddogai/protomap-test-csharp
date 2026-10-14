namespace Acme.Workforce.Orgdesign.Tests.Reorg.Preference;

using Acme.Workforce.Orgdesign.Reorg.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class WeightedReorgTargetViewChecks
{
    public static void WeightedReorgTargetViewShowsTheStatusInCapitals()
    {
        var item = WeightedReorgTarget.NewWeightedReorgTarget("code-324", "label-262");
        var view = WeightedReorgTargetView.ToWeightedReorgTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedReorgTargetView.ToWeightedReorgTargetViews(new List<WeightedReorgTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedReorgTargetViewShowsTheStatusInCapitals();
    }
}
