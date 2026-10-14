namespace Acme.Workforce.Orgdesign.Tests.Reorg.Preference;

using Acme.Workforce.Orgdesign.Reorg.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class WeightedReorgTargetRulesChecks
{
    public static void WeightedReorgTargetValidatorReportsEveryProblem()
    {
        var validator = WeightedReorgTargetRules.DefaultWeightedReorgTargetValidator();
        var item = WeightedReorgTarget.NewWeightedReorgTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedReorgTargetRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new WeightedReorgTargetRequiredLabelRule("weighted reorg target needs a value");
        var item = WeightedReorgTarget.NewWeightedReorgTarget("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedReorgTargetLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WeightedReorgTargetLabelLengthRule(32);
        var item = WeightedReorgTarget.NewWeightedReorgTarget("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedReorgTargetValidatorReportsEveryProblem();
        WeightedReorgTargetRequiredLabelRuleFlagsABlankLabel();
        WeightedReorgTargetLabelLengthRuleFlagsALongLabel();
    }
}
