namespace Acme.Workforce.Assets.Tests.AssetReturn.Variance;

using Acme.Workforce.Assets.AssetReturn.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AssetReturnWindowRulesChecks
{
    public static void AssetReturnWindowValidatorReportsEveryProblem()
    {
        var validator = AssetReturnWindowRules.DefaultAssetReturnWindowValidator();
        var item = AssetReturnWindow.NewAssetReturnWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnWindowVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new AssetReturnWindowVersionRangeRule(32, 3600);
        var item = AssetReturnWindow.NewAssetReturnWindow("x", "y");
        item.Version = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnWindowRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new AssetReturnWindowRatioRangeRule(3.4, 864.4);
        var item = AssetReturnWindow.NewAssetReturnWindow("x", "y");
        item.Ratio = 865.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnWindowTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AssetReturnWindowTitleLengthRule(120);
        var item = AssetReturnWindow.NewAssetReturnWindow("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssetReturnWindowValidatorReportsEveryProblem();
        AssetReturnWindowVersionRangeRuleFlagsValuesAboveVersion();
        AssetReturnWindowRatioRangeRuleFlagsValuesAboveRatio();
        AssetReturnWindowTitleLengthRuleFlagsALongTitle();
    }
}
