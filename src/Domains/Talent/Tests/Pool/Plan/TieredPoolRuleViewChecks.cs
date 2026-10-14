namespace Acme.Workforce.Talent.Tests.Pool.Plan;

using Acme.Workforce.Talent.Pool.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class TieredPoolRuleViewChecks
{
    public static void TieredPoolRuleViewShowsTheStatusInCapitals()
    {
        var item = TieredPoolRule.NewTieredPoolRule("code-36", "label-451");
        var view = TieredPoolRuleView.ToTieredPoolRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredPoolRuleView.ToTieredPoolRuleViews(new List<TieredPoolRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredPoolRuleViewShowsTheStatusInCapitals();
    }
}
