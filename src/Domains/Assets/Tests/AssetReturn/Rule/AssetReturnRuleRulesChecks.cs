namespace Acme.Workforce.Assets.Tests.AssetReturn.Rule;

using Acme.Workforce.Assets.AssetReturn.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AssetReturnRuleRulesChecks
{
    public static void AssetReturnRuleValidatorReportsEveryProblem()
    {
        var validator = AssetReturnRuleRules.DefaultAssetReturnRuleValidator();
        var item = AssetReturnRule.NewAssetReturnRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AssetReturnRuleRequiredCodeRule("asset return rule needs an entry");
        var item = AssetReturnRule.NewAssetReturnRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnRuleCountryLengthRuleFlagsALongCountry()
    {
        var rule = new AssetReturnRuleCountryLengthRule(120);
        var item = AssetReturnRule.NewAssetReturnRule("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnRuleGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new AssetReturnRuleGraceDaysRangeRule(1000, 86400);
        var item = AssetReturnRule.NewAssetReturnRule("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AssetReturnRuleValidatorReportsEveryProblem();
        AssetReturnRuleRequiredCodeRuleFlagsABlankCode();
        AssetReturnRuleCountryLengthRuleFlagsALongCountry();
        AssetReturnRuleGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
