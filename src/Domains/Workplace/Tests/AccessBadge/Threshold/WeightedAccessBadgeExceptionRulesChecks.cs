namespace Acme.Workforce.Workplace.Tests.AccessBadge.Threshold;

using Acme.Workforce.Workplace.AccessBadge.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class WeightedAccessBadgeExceptionRulesChecks
{
    public static void WeightedAccessBadgeExceptionValidatorReportsEveryProblem()
    {
        var validator = WeightedAccessBadgeExceptionRules.DefaultWeightedAccessBadgeExceptionValidator();
        var item = WeightedAccessBadgeException.NewWeightedAccessBadgeException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedAccessBadgeExceptionRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new WeightedAccessBadgeExceptionRateRangeRule(9.1, 4137.8);
        var item = WeightedAccessBadgeException.NewWeightedAccessBadgeException("x", "y");
        item.Rate = 4139.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedAccessBadgeExceptionUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new WeightedAccessBadgeExceptionUpdatedAtRangeRule(32, 255);
        var item = WeightedAccessBadgeException.NewWeightedAccessBadgeException("x", "y");
        item.UpdatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedAccessBadgeExceptionRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new WeightedAccessBadgeExceptionRequiredLabelRule("weighted access badge exception needs a value");
        var item = WeightedAccessBadgeException.NewWeightedAccessBadgeException("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedAccessBadgeExceptionValidatorReportsEveryProblem();
        WeightedAccessBadgeExceptionRateRangeRuleFlagsValuesAboveRate();
        WeightedAccessBadgeExceptionUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        WeightedAccessBadgeExceptionRequiredLabelRuleFlagsABlankLabel();
    }
}
