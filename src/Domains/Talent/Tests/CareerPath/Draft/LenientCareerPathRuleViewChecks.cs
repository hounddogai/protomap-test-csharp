namespace Acme.Workforce.Talent.Tests.CareerPath.Draft;

using Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientCareerPathRuleViewChecks
{
    public static void LenientCareerPathRuleViewShowsTheStatusInCapitals()
    {
        var item = LenientCareerPathRule.NewLenientCareerPathRule("code-837", "name-685");
        var view = LenientCareerPathRuleView.ToLenientCareerPathRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientCareerPathRuleView.ToLenientCareerPathRuleViews(new List<LenientCareerPathRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientCareerPathRuleViewShowsTheStatusInCapitals();
    }
}
