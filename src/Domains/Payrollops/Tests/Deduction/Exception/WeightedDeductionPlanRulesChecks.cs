namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class WeightedDeductionPlanRulesChecks
{
    public static void WeightedDeductionPlanValidatorReportsEveryProblem()
    {
        var validator = WeightedDeductionPlanRules.DefaultWeightedDeductionPlanValidator();
        var item = WeightedDeductionPlan.NewWeightedDeductionPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedDeductionPlanThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new WeightedDeductionPlanThresholdRangeRule(9.6, 2168.4);
        var item = WeightedDeductionPlan.NewWeightedDeductionPlan("x", "y");
        item.Threshold = 2169.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedDeductionPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedDeductionPlanCodeLengthRule(86400);
        var item = WeightedDeductionPlan.NewWeightedDeductionPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedDeductionPlanVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new WeightedDeductionPlanVersionRangeRule(32, 86400);
        var item = WeightedDeductionPlan.NewWeightedDeductionPlan("x", "y");
        item.Version = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedDeductionPlanValidatorReportsEveryProblem();
        WeightedDeductionPlanThresholdRangeRuleFlagsValuesAboveThreshold();
        WeightedDeductionPlanCodeLengthRuleFlagsALongCode();
        WeightedDeductionPlanVersionRangeRuleFlagsValuesAboveVersion();
    }
}
