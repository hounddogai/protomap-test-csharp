namespace Acme.Workforce.Workplace.Tests.Visitor.Cycle;

using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class DefaultVisitorRuleViewChecks
{
    public static void DefaultVisitorRuleViewShowsTheStatusInCapitals()
    {
        var item = DefaultVisitorRule.NewDefaultVisitorRule("code-619", "label-151");
        var view = DefaultVisitorRuleView.ToDefaultVisitorRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultVisitorRuleView.ToDefaultVisitorRuleViews(new List<DefaultVisitorRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultVisitorRuleViewShowsTheStatusInCapitals();
    }
}
