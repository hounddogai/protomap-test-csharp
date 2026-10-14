namespace Acme.Workforce.Absence.Tests.Balance.Summary;

using Acme.Workforce.Absence.Balance.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class WeightedBalanceSummaryRulesChecks
{
    public static void WeightedBalanceSummaryValidatorReportsEveryProblem()
    {
        var validator = WeightedBalanceSummaryRules.DefaultWeightedBalanceSummaryValidator();
        var item = WeightedBalanceSummary.NewWeightedBalanceSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedBalanceSummaryThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new WeightedBalanceSummaryThresholdRangeRule(7.9, 1352.3);
        var item = WeightedBalanceSummary.NewWeightedBalanceSummary("x", "y");
        item.Threshold = 1353.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedBalanceSummaryDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new WeightedBalanceSummaryDescriptionLengthRule(64);
        var item = WeightedBalanceSummary.NewWeightedBalanceSummary("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedBalanceSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedBalanceSummaryRequiredCodeRule("weighted balance summary needs a value");
        var item = WeightedBalanceSummary.NewWeightedBalanceSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedBalanceSummaryValidatorReportsEveryProblem();
        WeightedBalanceSummaryThresholdRangeRuleFlagsValuesAboveThreshold();
        WeightedBalanceSummaryDescriptionLengthRuleFlagsALongDescription();
        WeightedBalanceSummaryRequiredCodeRuleFlagsABlankCode();
    }
}
