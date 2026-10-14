namespace Acme.Workforce.Workplace.Tests.Visitor.Group;

using Acme.Workforce.Workplace.Visitor.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class VisitorRuleViewChecks
{
    public static void VisitorRuleViewShowsTheStatusInCapitals()
    {
        var item = VisitorRule.NewVisitorRule("code-632", "title-328");
        var view = VisitorRuleView.ToVisitorRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorRuleView.ToVisitorRuleViews(new List<VisitorRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorRuleViewShowsTheStatusInCapitals();
    }
}
