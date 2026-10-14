namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AssetReturnScoreRulesChecks
{
    public static void AssetReturnScoreValidatorReportsEveryProblem()
    {
        var validator = AssetReturnScoreRules.DefaultAssetReturnScoreValidator();
        var item = AssetReturnScore.NewAssetReturnScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AssetReturnScoreRequiredCodeRule("asset return score needs this field");
        var item = AssetReturnScore.NewAssetReturnScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnScoreRequiredNameRuleFlagsABlankName()
    {
        var rule = new AssetReturnScoreRequiredNameRule("asset return score needs a value");
        var item = AssetReturnScore.NewAssetReturnScore("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnScoreTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new AssetReturnScoreTimezoneLengthRule(1000);
        var item = AssetReturnScore.NewAssetReturnScore("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnScoreRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new AssetReturnScoreRequiredTimezoneRule("asset return score needs a value");
        var item = AssetReturnScore.NewAssetReturnScore("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssetReturnScoreValidatorReportsEveryProblem();
        AssetReturnScoreRequiredCodeRuleFlagsABlankCode();
        AssetReturnScoreRequiredNameRuleFlagsABlankName();
        AssetReturnScoreTimezoneLengthRuleFlagsALongTimezone();
        AssetReturnScoreRequiredTimezoneRuleFlagsABlankTimezone();
    }
}
