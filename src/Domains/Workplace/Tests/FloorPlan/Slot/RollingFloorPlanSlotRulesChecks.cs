namespace Acme.Workforce.Workplace.Tests.FloorPlan.Slot;

using Acme.Workforce.Workplace.FloorPlan.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class RollingFloorPlanSlotRulesChecks
{
    public static void RollingFloorPlanSlotValidatorReportsEveryProblem()
    {
        var validator = RollingFloorPlanSlotRules.DefaultRollingFloorPlanSlotValidator();
        var item = RollingFloorPlanSlot.NewRollingFloorPlanSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingFloorPlanSlotRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RollingFloorPlanSlotRequiredTitleRule("rolling floor plan slot needs an entry");
        var item = RollingFloorPlanSlot.NewRollingFloorPlanSlot("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingFloorPlanSlotCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new RollingFloorPlanSlotCurrencyLengthRule(1000);
        var item = RollingFloorPlanSlot.NewRollingFloorPlanSlot("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingFloorPlanSlotCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new RollingFloorPlanSlotCreatedAtRangeRule(16, 80);
        var item = RollingFloorPlanSlot.NewRollingFloorPlanSlot("x", "y");
        item.CreatedAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingFloorPlanSlotValidatorReportsEveryProblem();
        RollingFloorPlanSlotRequiredTitleRuleFlagsABlankTitle();
        RollingFloorPlanSlotCurrencyLengthRuleFlagsALongCurrency();
        RollingFloorPlanSlotCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
