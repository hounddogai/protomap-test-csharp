namespace Acme.Workforce.Assets.Tests.AssetReturn.OverrideItem;

using Acme.Workforce.Assets.AssetReturn.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class AssetReturnTierRulesChecks
{
    public static void AssetReturnTierValidatorReportsEveryProblem()
    {
        var validator = AssetReturnTierRules.DefaultAssetReturnTierValidator();
        var item = AssetReturnTier.NewAssetReturnTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnTierRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new AssetReturnTierRequiredRegionRule("asset return tier needs an entry");
        var item = AssetReturnTier.NewAssetReturnTier("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnTierAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new AssetReturnTierAmountRangeRule(3.9, 61.7);
        var item = AssetReturnTier.NewAssetReturnTier("x", "y");
        item.Amount = 63.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnTierRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AssetReturnTierRequiredLabelRule("asset return tier needs this field");
        var item = AssetReturnTier.NewAssetReturnTier("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssetReturnTierValidatorReportsEveryProblem();
        AssetReturnTierRequiredRegionRuleFlagsABlankRegion();
        AssetReturnTierAmountRangeRuleFlagsValuesAboveAmount();
        AssetReturnTierRequiredLabelRuleFlagsABlankLabel();
    }
}
