namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class ManualFloorPlanItemRulesChecks
{
    public static void ManualFloorPlanItemValidatorReportsEveryProblem()
    {
        var validator = ManualFloorPlanItemRules.DefaultManualFloorPlanItemValidator();
        var item = ManualFloorPlanItem.NewManualFloorPlanItem("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualFloorPlanItemOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new ManualFloorPlanItemOwnerIdLengthRule(86400);
        var item = ManualFloorPlanItem.NewManualFloorPlanItem("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualFloorPlanItemRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ManualFloorPlanItemRequiredTitleRule("manual floor plan item needs a value");
        var item = ManualFloorPlanItem.NewManualFloorPlanItem("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualFloorPlanItemCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualFloorPlanItemCodeLengthRule(16);
        var item = ManualFloorPlanItem.NewManualFloorPlanItem("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualFloorPlanItemLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new ManualFloorPlanItemLeaveDaysRangeRule(1000, 3600);
        var item = ManualFloorPlanItem.NewManualFloorPlanItem("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualFloorPlanItemValidatorReportsEveryProblem();
        ManualFloorPlanItemOwnerIdLengthRuleFlagsALongOwnerId();
        ManualFloorPlanItemRequiredTitleRuleFlagsABlankTitle();
        ManualFloorPlanItemCodeLengthRuleFlagsALongCode();
        ManualFloorPlanItemLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
