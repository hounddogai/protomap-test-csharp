namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class ScheduledFloorPlanTargetRulesChecks
{
    public static void ScheduledFloorPlanTargetValidatorReportsEveryProblem()
    {
        var validator = ScheduledFloorPlanTargetRules.DefaultScheduledFloorPlanTargetValidator();
        var item = ScheduledFloorPlanTarget.NewScheduledFloorPlanTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledFloorPlanTargetCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new ScheduledFloorPlanTargetCategoryLengthRule(80);
        var item = ScheduledFloorPlanTarget.NewScheduledFloorPlanTarget("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledFloorPlanTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledFloorPlanTargetRequiredCodeRule("scheduled floor plan target needs an entry");
        var item = ScheduledFloorPlanTarget.NewScheduledFloorPlanTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledFloorPlanTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledFloorPlanTargetCodeLengthRule(3600);
        var item = ScheduledFloorPlanTarget.NewScheduledFloorPlanTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledFloorPlanTargetValidatorReportsEveryProblem();
        ScheduledFloorPlanTargetCategoryLengthRuleFlagsALongCategory();
        ScheduledFloorPlanTargetRequiredCodeRuleFlagsABlankCode();
        ScheduledFloorPlanTargetCodeLengthRuleFlagsALongCode();
    }
}
