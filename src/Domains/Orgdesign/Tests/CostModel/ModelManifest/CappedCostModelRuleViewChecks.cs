namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelManifest;

using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Checks of model manifest code, run by the project's runner.</summary>
public static class CappedCostModelRuleViewChecks
{
    public static void CappedCostModelRuleViewShowsTheStatusInCapitals()
    {
        var item = CappedCostModelRule.NewCappedCostModelRule("code-328", "name-873");
        var view = CappedCostModelRuleView.ToCappedCostModelRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedCostModelRuleView.ToCappedCostModelRuleViews(new List<CappedCostModelRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedCostModelRuleViewShowsTheStatusInCapitals();
    }
}
