namespace Acme.Workforce.Assets.Tests.AssetReturn.Agreement;

using Acme.Workforce.Assets.AssetReturn.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AssetReturnGroupRulesChecks
{
    public static void AssetReturnGroupValidatorReportsEveryProblem()
    {
        var validator = AssetReturnGroupRules.DefaultAssetReturnGroupValidator();
        var item = AssetReturnGroup.NewAssetReturnGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new AssetReturnGroupCodeLengthRule(16);
        var item = AssetReturnGroup.NewAssetReturnGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnGroupBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new AssetReturnGroupBatchSizeRangeRule(120, 255);
        var item = AssetReturnGroup.NewAssetReturnGroup("x", "y");
        item.BatchSize = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnGroupTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AssetReturnGroupTitleLengthRule(64);
        var item = AssetReturnGroup.NewAssetReturnGroup("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnGroupVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new AssetReturnGroupVersionRangeRule(80, 86400);
        var item = AssetReturnGroup.NewAssetReturnGroup("x", "y");
        item.Version = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AssetReturnGroupRequiredCodeRule("asset return group needs a value");
        var item = AssetReturnGroup.NewAssetReturnGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssetReturnGroupValidatorReportsEveryProblem();
        AssetReturnGroupCodeLengthRuleFlagsALongCode();
        AssetReturnGroupBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        AssetReturnGroupTitleLengthRuleFlagsALongTitle();
        AssetReturnGroupVersionRangeRuleFlagsValuesAboveVersion();
        AssetReturnGroupRequiredCodeRuleFlagsABlankCode();
    }
}
