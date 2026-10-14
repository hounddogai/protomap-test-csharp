namespace Acme.Workforce.Talent.Tests.Pool.Rule;

using Acme.Workforce.Talent.Pool.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ScheduledPoolRuleViewChecks
{
    public static void ScheduledPoolRuleViewShowsTheStatusInCapitals()
    {
        var item = ScheduledPoolRule.NewScheduledPoolRule("code-284", "title-998");
        var view = ScheduledPoolRuleView.ToScheduledPoolRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledPoolRuleView.ToScheduledPoolRuleViews(new List<ScheduledPoolRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledPoolRuleViewShowsTheStatusInCapitals();
    }
}
