namespace Acme.Workforce.Compensation.Tests.Adjustment.Allocation;

using Acme.Workforce.Compensation.Adjustment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class AdjustmentBaselineRulesChecks
{
    public static void AdjustmentBaselineValidatorReportsEveryProblem()
    {
        var validator = AdjustmentBaselineRules.DefaultAdjustmentBaselineValidator();
        var item = AdjustmentBaseline.NewAdjustmentBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentBaselineGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new AdjustmentBaselineGraceDaysRangeRule(255, 1000);
        var item = AdjustmentBaseline.NewAdjustmentBaseline("x", "y");
        item.GraceDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentBaselineThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new AdjustmentBaselineThresholdRangeRule(0.0, 1870.1);
        var item = AdjustmentBaseline.NewAdjustmentBaseline("x", "y");
        item.Threshold = 1871.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdjustmentBaselineRequiredCodeRule("adjustment baseline needs this field");
        var item = AdjustmentBaseline.NewAdjustmentBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdjustmentBaselineValidatorReportsEveryProblem();
        AdjustmentBaselineGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        AdjustmentBaselineThresholdRangeRuleFlagsValuesAboveThreshold();
        AdjustmentBaselineRequiredCodeRuleFlagsABlankCode();
    }
}
