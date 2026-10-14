namespace Acme.Workforce.Orgdesign.Tests.Reorg.Threshold;

using Acme.Workforce.Orgdesign.Reorg.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class WeightedReorgRuleViewChecks
{
    public static void WeightedReorgRuleViewShowsTheStatusInCapitals()
    {
        var item = WeightedReorgRule.NewWeightedReorgRule("code-370", "name-700");
        var view = WeightedReorgRuleView.ToWeightedReorgRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedReorgRuleView.ToWeightedReorgRuleViews(new List<WeightedReorgRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedReorgRuleViewShowsTheStatusInCapitals();
    }
}
