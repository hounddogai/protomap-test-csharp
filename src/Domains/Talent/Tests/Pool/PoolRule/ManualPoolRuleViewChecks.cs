namespace Acme.Workforce.Talent.Tests.Pool.PoolRule;

using Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>Checks of pool rule code, run by the project's runner.</summary>
public static class ManualPoolRuleViewChecks
{
    public static void ManualPoolRuleViewShowsTheStatusInCapitals()
    {
        var item = ManualPoolRule.NewManualPoolRule("code-350", "name-673");
        var view = ManualPoolRuleView.ToManualPoolRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualPoolRuleView.ToManualPoolRuleViews(new List<ManualPoolRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualPoolRuleViewShowsTheStatusInCapitals();
    }
}
