namespace Acme.Workforce.Orgdesign.Tests.Headcount.Budget;

using Acme.Workforce.Orgdesign.Headcount.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class HeadcountRuleViewChecks
{
    public static void HeadcountRuleViewShowsTheStatusInCapitals()
    {
        var item = HeadcountRule.NewHeadcountRule("code-661", "name-957");
        var view = HeadcountRuleView.ToHeadcountRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HeadcountRuleView.ToHeadcountRuleViews(new List<HeadcountRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HeadcountRuleViewShowsTheStatusInCapitals();
    }
}
