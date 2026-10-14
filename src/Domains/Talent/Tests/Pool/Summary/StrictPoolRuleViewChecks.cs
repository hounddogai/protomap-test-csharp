namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StrictPoolRuleViewChecks
{
    public static void StrictPoolRuleViewShowsTheStatusInCapitals()
    {
        var item = StrictPoolRule.NewStrictPoolRule("code-155", "title-975");
        var view = StrictPoolRuleView.ToStrictPoolRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictPoolRuleView.ToStrictPoolRuleViews(new List<StrictPoolRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictPoolRuleViewShowsTheStatusInCapitals();
    }
}
