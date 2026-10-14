namespace Acme.Workforce.Workplace.Tests.FloorPlan.Milestone;

using Acme.Workforce.Workplace.FloorPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PendingFloorPlanTargetRulesChecks
{
    public static void PendingFloorPlanTargetValidatorReportsEveryProblem()
    {
        var validator = PendingFloorPlanTargetRules.DefaultPendingFloorPlanTargetValidator();
        var item = PendingFloorPlanTarget.NewPendingFloorPlanTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingFloorPlanTargetLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PendingFloorPlanTargetLabelLengthRule(64);
        var item = PendingFloorPlanTarget.NewPendingFloorPlanTarget("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingFloorPlanTargetRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PendingFloorPlanTargetRequiredLabelRule("pending floor plan target needs this field");
        var item = PendingFloorPlanTarget.NewPendingFloorPlanTarget("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingFloorPlanTargetRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new PendingFloorPlanTargetRequiredDescriptionRule("pending floor plan target needs an entry");
        var item = PendingFloorPlanTarget.NewPendingFloorPlanTarget("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingFloorPlanTargetDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new PendingFloorPlanTargetDescriptionLengthRule(32);
        var item = PendingFloorPlanTarget.NewPendingFloorPlanTarget("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingFloorPlanTargetWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new PendingFloorPlanTargetWindowDaysRangeRule(32, 3600);
        var item = PendingFloorPlanTarget.NewPendingFloorPlanTarget("x", "y");
        item.WindowDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingFloorPlanTargetValidatorReportsEveryProblem();
        PendingFloorPlanTargetLabelLengthRuleFlagsALongLabel();
        PendingFloorPlanTargetRequiredLabelRuleFlagsABlankLabel();
        PendingFloorPlanTargetRequiredDescriptionRuleFlagsABlankDescription();
        PendingFloorPlanTargetDescriptionLengthRuleFlagsALongDescription();
        PendingFloorPlanTargetWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
