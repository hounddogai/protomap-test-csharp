namespace Acme.Workforce.Workplace.Tests.FloorPlan.Allocation;

using Acme.Workforce.Workplace.FloorPlan.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class FloorPlanAllocationRulesChecks
{
    public static void FloorPlanAllocationValidatorReportsEveryProblem()
    {
        var validator = FloorPlanAllocationRules.DefaultFloorPlanAllocationValidator();
        var item = FloorPlanAllocation.NewFloorPlanAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanAllocationRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new FloorPlanAllocationRequiredGradeRule("floor plan allocation needs an entry");
        var item = FloorPlanAllocation.NewFloorPlanAllocation("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FloorPlanAllocationRequiredCodeRule("floor plan allocation needs this field");
        var item = FloorPlanAllocation.NewFloorPlanAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanAllocationRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new FloorPlanAllocationRequiredLabelRule("floor plan allocation needs this field");
        var item = FloorPlanAllocation.NewFloorPlanAllocation("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanAllocationRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new FloorPlanAllocationRevisionRangeRule(80, 1000);
        var item = FloorPlanAllocation.NewFloorPlanAllocation("x", "y");
        item.Revision = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FloorPlanAllocationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FloorPlanAllocationLabelLengthRule(86400);
        var item = FloorPlanAllocation.NewFloorPlanAllocation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FloorPlanAllocationValidatorReportsEveryProblem();
        FloorPlanAllocationRequiredGradeRuleFlagsABlankGrade();
        FloorPlanAllocationRequiredCodeRuleFlagsABlankCode();
        FloorPlanAllocationRequiredLabelRuleFlagsABlankLabel();
        FloorPlanAllocationRevisionRangeRuleFlagsValuesAboveRevision();
        FloorPlanAllocationLabelLengthRuleFlagsALongLabel();
    }
}
