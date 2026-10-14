namespace Acme.Workforce.Assets.Tests.AssetReturn.Segment;

using Acme.Workforce.Assets.AssetReturn.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class LegacyAssetReturnRuleViewChecks
{
    public static void LegacyAssetReturnRuleViewShowsTheStatusInCapitals()
    {
        var item = LegacyAssetReturnRule.NewLegacyAssetReturnRule("code-354", "name-288");
        var view = LegacyAssetReturnRuleView.ToLegacyAssetReturnRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyAssetReturnRuleView.ToLegacyAssetReturnRuleViews(new List<LegacyAssetReturnRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyAssetReturnRuleViewShowsTheStatusInCapitals();
    }
}
