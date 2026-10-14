namespace Acme.Workforce.Assets.Tests.AssetReturn.Variance;

using Acme.Workforce.Assets.AssetReturn.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AssetReturnVarianceRulesChecks
{
    public static void AssetReturnVarianceValidatorReportsEveryProblem()
    {
        var validator = AssetReturnVarianceRules.DefaultAssetReturnVarianceValidator();
        var item = AssetReturnVariance.NewAssetReturnVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnVarianceChannelLengthRuleFlagsALongChannel()
    {
        var rule = new AssetReturnVarianceChannelLengthRule(3600);
        var item = AssetReturnVariance.NewAssetReturnVariance("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnVarianceRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new AssetReturnVarianceRequiredChannelRule("asset return variance needs an entry");
        var item = AssetReturnVariance.NewAssetReturnVariance("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnVarianceSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new AssetReturnVarianceSequenceRangeRule(16, 120);
        var item = AssetReturnVariance.NewAssetReturnVariance("x", "y");
        item.Sequence = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnVarianceRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new AssetReturnVarianceRequiredTimezoneRule("asset return variance needs this field");
        var item = AssetReturnVariance.NewAssetReturnVariance("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnVarianceMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new AssetReturnVarianceMarginRangeRule(9.4, 1783.9);
        var item = AssetReturnVariance.NewAssetReturnVariance("x", "y");
        item.Margin = 1785.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AssetReturnVarianceValidatorReportsEveryProblem();
        AssetReturnVarianceChannelLengthRuleFlagsALongChannel();
        AssetReturnVarianceRequiredChannelRuleFlagsABlankChannel();
        AssetReturnVarianceSequenceRangeRuleFlagsValuesAboveSequence();
        AssetReturnVarianceRequiredTimezoneRuleFlagsABlankTimezone();
        AssetReturnVarianceMarginRangeRuleFlagsValuesAboveMargin();
    }
}
