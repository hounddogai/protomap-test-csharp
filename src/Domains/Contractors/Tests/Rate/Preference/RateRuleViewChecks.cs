namespace Acme.Workforce.Contractors.Tests.Rate.Preference;

using Acme.Workforce.Contractors.Rate.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RateRuleViewChecks
{
    public static void RateRuleViewShowsTheStatusInCapitals()
    {
        var item = RateRule.NewRateRule("code-87", "name-441");
        var view = RateRuleView.ToRateRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RateRuleView.ToRateRuleViews(new List<RateRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RateRuleViewShowsTheStatusInCapitals();
    }
}
