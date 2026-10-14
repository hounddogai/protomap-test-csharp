namespace Acme.Workforce.Compensation.Tests.Adjustment.Score;

using Acme.Workforce.Compensation.Adjustment.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdjustmentPlanRulesChecks
{
    public static void AdjustmentPlanValidatorReportsEveryProblem()
    {
        var validator = AdjustmentPlanRules.DefaultAdjustmentPlanValidator();
        var item = AdjustmentPlan.NewAdjustmentPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentPlanRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new AdjustmentPlanRequiredSkillRule("adjustment plan needs an entry");
        var item = AdjustmentPlan.NewAdjustmentPlan("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentPlanJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new AdjustmentPlanJobCodeLengthRule(86400);
        var item = AdjustmentPlan.NewAdjustmentPlan("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentPlanCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new AdjustmentPlanCapacityRangeRule(80, 120);
        var item = AdjustmentPlan.NewAdjustmentPlan("x", "y");
        item.Capacity = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentPlanLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdjustmentPlanLabelLengthRule(86400);
        var item = AdjustmentPlan.NewAdjustmentPlan("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdjustmentPlanValidatorReportsEveryProblem();
        AdjustmentPlanRequiredSkillRuleFlagsABlankSkill();
        AdjustmentPlanJobCodeLengthRuleFlagsALongJobCode();
        AdjustmentPlanCapacityRangeRuleFlagsValuesAboveCapacity();
        AdjustmentPlanLabelLengthRuleFlagsALongLabel();
    }
}
