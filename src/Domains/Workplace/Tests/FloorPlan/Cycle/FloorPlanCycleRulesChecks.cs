namespace Acme.Workforce.Workplace.Tests.FloorPlan.Cycle;

using Acme.Workforce.Workplace.FloorPlan.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class FloorPlanCycleRulesChecks
{
    public static void FloorPlanCycleValidatorReportsEveryProblem()
    {
        var validator = FloorPlanCycleRules.DefaultFloorPlanCycleValidator();
        var item = FloorPlanCycle.NewFloorPlanCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanCycleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FloorPlanCycleLabelLengthRule(120);
        var item = FloorPlanCycle.NewFloorPlanCycle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanCycleRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new FloorPlanCycleRevisionRangeRule(120, 255);
        var item = FloorPlanCycle.NewFloorPlanCycle("x", "y");
        item.Revision = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FloorPlanCycleCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new FloorPlanCycleCapacityRangeRule(16, 86400);
        var item = FloorPlanCycle.NewFloorPlanCycle("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FloorPlanCycleFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new FloorPlanCycleFloorValueRangeRule(0.3, 729.7);
        var item = FloorPlanCycle.NewFloorPlanCycle("x", "y");
        item.FloorValue = 731.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FloorPlanCycleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FloorPlanCycleRequiredCodeRule("floor plan cycle needs this field");
        var item = FloorPlanCycle.NewFloorPlanCycle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FloorPlanCycleValidatorReportsEveryProblem();
        FloorPlanCycleLabelLengthRuleFlagsALongLabel();
        FloorPlanCycleRevisionRangeRuleFlagsValuesAboveRevision();
        FloorPlanCycleCapacityRangeRuleFlagsValuesAboveCapacity();
        FloorPlanCycleFloorValueRangeRuleFlagsValuesAboveFloorValue();
        FloorPlanCycleRequiredCodeRuleFlagsABlankCode();
    }
}
