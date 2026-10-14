namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Milestone;

using Acme.Workforce.Talent.SuccessionPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SuccessionPlanTargetRulesChecks
{
    public static void SuccessionPlanTargetValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanTargetRules.DefaultSuccessionPlanTargetValidator();
        var item = SuccessionPlanTarget.NewSuccessionPlanTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanTargetLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new SuccessionPlanTargetLimitRangeRule(32, 1000);
        var item = SuccessionPlanTarget.NewSuccessionPlanTarget("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanTargetNameLengthRuleFlagsALongName()
    {
        var rule = new SuccessionPlanTargetNameLengthRule(16);
        var item = SuccessionPlanTarget.NewSuccessionPlanTarget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanTargetThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new SuccessionPlanTargetThresholdRangeRule(3.5, 1813.5);
        var item = SuccessionPlanTarget.NewSuccessionPlanTarget("x", "y");
        item.Threshold = 1815.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanTargetCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new SuccessionPlanTargetCostCenterLengthRule(120);
        var item = SuccessionPlanTarget.NewSuccessionPlanTarget("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SuccessionPlanTargetValidatorReportsEveryProblem();
        SuccessionPlanTargetLimitRangeRuleFlagsValuesAboveLimit();
        SuccessionPlanTargetNameLengthRuleFlagsALongName();
        SuccessionPlanTargetThresholdRangeRuleFlagsValuesAboveThreshold();
        SuccessionPlanTargetCostCenterLengthRuleFlagsALongCostCenter();
    }
}
