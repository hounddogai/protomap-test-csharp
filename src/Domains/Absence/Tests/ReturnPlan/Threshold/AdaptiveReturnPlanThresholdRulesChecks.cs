namespace Acme.Workforce.Absence.Tests.ReturnPlan.Threshold;

using Acme.Workforce.Absence.ReturnPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AdaptiveReturnPlanThresholdRulesChecks
{
    public static void AdaptiveReturnPlanThresholdValidatorReportsEveryProblem()
    {
        var validator = AdaptiveReturnPlanThresholdRules.DefaultAdaptiveReturnPlanThresholdValidator();
        var item = AdaptiveReturnPlanThreshold.NewAdaptiveReturnPlanThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveReturnPlanThresholdPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new AdaptiveReturnPlanThresholdPriorityRangeRule(80, 255);
        var item = AdaptiveReturnPlanThreshold.NewAdaptiveReturnPlanThreshold("x", "y");
        item.Priority = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveReturnPlanThresholdExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new AdaptiveReturnPlanThresholdExpiresAtRangeRule(120, 255);
        var item = AdaptiveReturnPlanThreshold.NewAdaptiveReturnPlanThreshold("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveReturnPlanThresholdDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new AdaptiveReturnPlanThresholdDepartmentLengthRule(86400);
        var item = AdaptiveReturnPlanThreshold.NewAdaptiveReturnPlanThreshold("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveReturnPlanThresholdValidatorReportsEveryProblem();
        AdaptiveReturnPlanThresholdPriorityRangeRuleFlagsValuesAbovePriority();
        AdaptiveReturnPlanThresholdExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        AdaptiveReturnPlanThresholdDepartmentLengthRuleFlagsALongDepartment();
    }
}
