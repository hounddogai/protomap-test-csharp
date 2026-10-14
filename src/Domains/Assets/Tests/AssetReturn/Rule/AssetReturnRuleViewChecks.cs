namespace Acme.Workforce.Assets.Tests.AssetReturn.Rule;

using Acme.Workforce.Assets.AssetReturn.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AssetReturnRuleViewChecks
{
    public static void AssetReturnRuleViewShowsTheStatusInCapitals()
    {
        var item = AssetReturnRule.NewAssetReturnRule("code-965", "label-501");
        var view = AssetReturnRuleView.ToAssetReturnRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssetReturnRuleView.ToAssetReturnRuleViews(new List<AssetReturnRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssetReturnRuleViewShowsTheStatusInCapitals();
    }
}
