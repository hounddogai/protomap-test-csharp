namespace Acme.Workforce.Talent.Tests.CareerPath.Rule;

using Acme.Workforce.Talent.CareerPath.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class CareerPathRuleViewChecks
{
    public static void CareerPathRuleViewShowsTheStatusInCapitals()
    {
        var item = CareerPathRule.NewCareerPathRule("code-145", "title-248");
        var view = CareerPathRuleView.ToCareerPathRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CareerPathRuleView.ToCareerPathRuleViews(new List<CareerPathRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CareerPathRuleViewShowsTheStatusInCapitals();
    }
}
