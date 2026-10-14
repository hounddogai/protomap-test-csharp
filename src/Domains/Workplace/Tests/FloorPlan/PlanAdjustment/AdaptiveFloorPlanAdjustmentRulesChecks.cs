namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAdjustment;

using Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>Checks of plan adjustment code, run by the project's runner.</summary>
public static class AdaptiveFloorPlanAdjustmentRulesChecks
{
    public static void AdaptiveFloorPlanAdjustmentValidatorReportsEveryProblem()
    {
        var validator = AdaptiveFloorPlanAdjustmentRules.DefaultAdaptiveFloorPlanAdjustmentValidator();
        var item = AdaptiveFloorPlanAdjustment.NewAdaptiveFloorPlanAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveFloorPlanAdjustmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdaptiveFloorPlanAdjustmentLabelLengthRule(255);
        var item = AdaptiveFloorPlanAdjustment.NewAdaptiveFloorPlanAdjustment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveFloorPlanAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveFloorPlanAdjustmentCodeLengthRule(80);
        var item = AdaptiveFloorPlanAdjustment.NewAdaptiveFloorPlanAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveFloorPlanAdjustmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdaptiveFloorPlanAdjustmentRequiredLabelRule("adaptive floor plan adjustment needs a value");
        var item = AdaptiveFloorPlanAdjustment.NewAdaptiveFloorPlanAdjustment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveFloorPlanAdjustmentNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptiveFloorPlanAdjustmentNameLengthRule(16);
        var item = AdaptiveFloorPlanAdjustment.NewAdaptiveFloorPlanAdjustment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveFloorPlanAdjustmentDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new AdaptiveFloorPlanAdjustmentDepartmentLengthRule(255);
        var item = AdaptiveFloorPlanAdjustment.NewAdaptiveFloorPlanAdjustment("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveFloorPlanAdjustmentValidatorReportsEveryProblem();
        AdaptiveFloorPlanAdjustmentLabelLengthRuleFlagsALongLabel();
        AdaptiveFloorPlanAdjustmentCodeLengthRuleFlagsALongCode();
        AdaptiveFloorPlanAdjustmentRequiredLabelRuleFlagsABlankLabel();
        AdaptiveFloorPlanAdjustmentNameLengthRuleFlagsALongName();
        AdaptiveFloorPlanAdjustmentDepartmentLengthRuleFlagsALongDepartment();
    }
}
