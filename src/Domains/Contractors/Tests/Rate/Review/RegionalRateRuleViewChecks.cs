namespace Acme.Workforce.Contractors.Tests.Rate.Review;

using Acme.Workforce.Contractors.Rate.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RegionalRateRuleViewChecks
{
    public static void RegionalRateRuleViewShowsTheStatusInCapitals()
    {
        var item = RegionalRateRule.NewRegionalRateRule("code-372", "title-729");
        var view = RegionalRateRuleView.ToRegionalRateRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalRateRuleView.ToRegionalRateRuleViews(new List<RegionalRateRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalRateRuleViewShowsTheStatusInCapitals();
    }
}
