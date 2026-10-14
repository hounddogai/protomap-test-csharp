namespace Acme.Workforce.Talent.Tests.Pool.Audit;

using Acme.Workforce.Talent.Pool.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PoolRuleViewChecks
{
    public static void PoolRuleViewShowsTheStatusInCapitals()
    {
        var item = PoolRule.NewPoolRule("code-950", "label-949");
        var view = PoolRuleView.ToPoolRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PoolRuleView.ToPoolRuleViews(new List<PoolRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PoolRuleViewShowsTheStatusInCapitals();
    }
}
