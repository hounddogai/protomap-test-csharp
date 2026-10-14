namespace Acme.Workforce.Orgdesign.Tests.CostModel.Score;

using Acme.Workforce.Orgdesign.CostModel.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class CostModelRuleViewChecks
{
    public static void CostModelRuleViewShowsTheStatusInCapitals()
    {
        var item = CostModelRule.NewCostModelRule("code-674", "label-140");
        var view = CostModelRuleView.ToCostModelRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CostModelRuleView.ToCostModelRuleViews(new List<CostModelRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CostModelRuleViewShowsTheStatusInCapitals();
    }
}
