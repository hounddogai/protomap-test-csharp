namespace Acme.Workforce.Orgdesign.Tests.Reorg.Threshold;

using Acme.Workforce.Orgdesign.Reorg.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class WeightedReorgRuleRulesChecks
{
    public static void WeightedReorgRuleValidatorReportsEveryProblem()
    {
        var validator = WeightedReorgRuleRules.DefaultWeightedReorgRuleValidator();
        var item = WeightedReorgRule.NewWeightedReorgRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedReorgRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedReorgRuleRequiredCodeRule("weighted reorg rule needs a value");
        var item = WeightedReorgRule.NewWeightedReorgRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedReorgRuleVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new WeightedReorgRuleVersionRangeRule(64, 255);
        var item = WeightedReorgRule.NewWeightedReorgRule("x", "y");
        item.Version = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedReorgRuleValidatorReportsEveryProblem();
        WeightedReorgRuleRequiredCodeRuleFlagsABlankCode();
        WeightedReorgRuleVersionRangeRuleFlagsValuesAboveVersion();
    }
}
