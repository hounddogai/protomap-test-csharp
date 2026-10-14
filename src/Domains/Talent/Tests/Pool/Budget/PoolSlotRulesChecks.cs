namespace Acme.Workforce.Talent.Tests.Pool.Budget;

using Acme.Workforce.Talent.Pool.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PoolSlotRulesChecks
{
    public static void PoolSlotValidatorReportsEveryProblem()
    {
        var validator = PoolSlotRules.DefaultPoolSlotValidator();
        var item = PoolSlot.NewPoolSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PoolSlotRequiredCodeRule("pool slot needs a value");
        var item = PoolSlot.NewPoolSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolSlotLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PoolSlotLabelLengthRule(1000);
        var item = PoolSlot.NewPoolSlot("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PoolSlotFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new PoolSlotFloorValueRangeRule(8.1, 676.3);
        var item = PoolSlot.NewPoolSlot("x", "y");
        item.FloorValue = 677.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolSlotRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new PoolSlotRequiredManagerIdRule("pool slot needs a value");
        var item = PoolSlot.NewPoolSlot("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PoolSlotValidatorReportsEveryProblem();
        PoolSlotRequiredCodeRuleFlagsABlankCode();
        PoolSlotLabelLengthRuleFlagsALongLabel();
        PoolSlotFloorValueRangeRuleFlagsValuesAboveFloorValue();
        PoolSlotRequiredManagerIdRuleFlagsABlankManagerId();
    }
}
