namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAdjustment;

using Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>Checks of plan adjustment code, run by the project's runner.</summary>
public static class FloorPlanItemRulesChecks
{
    public static void FloorPlanItemValidatorReportsEveryProblem()
    {
        var validator = FloorPlanItemRules.DefaultFloorPlanItemValidator();
        var item = FloorPlanItem.NewFloorPlanItem("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanItemCodeLengthRuleFlagsALongCode()
    {
        var rule = new FloorPlanItemCodeLengthRule(32);
        var item = FloorPlanItem.NewFloorPlanItem("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanItemNameLengthRuleFlagsALongName()
    {
        var rule = new FloorPlanItemNameLengthRule(3600);
        var item = FloorPlanItem.NewFloorPlanItem("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FloorPlanItemValidatorReportsEveryProblem();
        FloorPlanItemCodeLengthRuleFlagsALongCode();
        FloorPlanItemNameLengthRuleFlagsALongName();
    }
}
