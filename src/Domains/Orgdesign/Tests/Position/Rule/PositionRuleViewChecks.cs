namespace Acme.Workforce.Orgdesign.Tests.Position.Rule;

using Acme.Workforce.Orgdesign.Position.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PositionRuleViewChecks
{
    public static void PositionRuleViewShowsTheStatusInCapitals()
    {
        var item = PositionRule.NewPositionRule("code-185", "label-752");
        var view = PositionRuleView.ToPositionRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PositionRuleView.ToPositionRuleViews(new List<PositionRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PositionRuleViewShowsTheStatusInCapitals();
    }
}
