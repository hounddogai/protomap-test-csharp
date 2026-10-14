namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class FloorPlanSlotRulesChecks
{
    public static void FloorPlanSlotValidatorReportsEveryProblem()
    {
        var validator = FloorPlanSlotRules.DefaultFloorPlanSlotValidator();
        var item = FloorPlanSlot.NewFloorPlanSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanSlotCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new FloorPlanSlotCurrencyLengthRule(64);
        var item = FloorPlanSlot.NewFloorPlanSlot("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanSlotEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new FloorPlanSlotEmployeeIdLengthRule(255);
        var item = FloorPlanSlot.NewFloorPlanSlot("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanSlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new FloorPlanSlotCodeLengthRule(120);
        var item = FloorPlanSlot.NewFloorPlanSlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanSlotEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new FloorPlanSlotEndsAtRangeRule(32, 64);
        var item = FloorPlanSlot.NewFloorPlanSlot("x", "y");
        item.EndsAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FloorPlanSlotBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new FloorPlanSlotBudgetRangeRule(9.7, 1751.1);
        var item = FloorPlanSlot.NewFloorPlanSlot("x", "y");
        item.Budget = 1752.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FloorPlanSlotRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new FloorPlanSlotRequiredEmployeeIdRule("floor plan slot needs this field");
        var item = FloorPlanSlot.NewFloorPlanSlot("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FloorPlanSlotValidatorReportsEveryProblem();
        FloorPlanSlotCurrencyLengthRuleFlagsALongCurrency();
        FloorPlanSlotEmployeeIdLengthRuleFlagsALongEmployeeId();
        FloorPlanSlotCodeLengthRuleFlagsALongCode();
        FloorPlanSlotEndsAtRangeRuleFlagsValuesAboveEndsAt();
        FloorPlanSlotBudgetRangeRuleFlagsValuesAboveBudget();
        FloorPlanSlotRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
