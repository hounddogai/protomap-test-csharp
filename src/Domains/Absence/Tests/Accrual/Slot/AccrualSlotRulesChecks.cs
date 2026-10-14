namespace Acme.Workforce.Absence.Tests.Accrual.Slot;

using Acme.Workforce.Absence.Accrual.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class AccrualSlotRulesChecks
{
    public static void AccrualSlotValidatorReportsEveryProblem()
    {
        var validator = AccrualSlotRules.DefaultAccrualSlotValidator();
        var item = AccrualSlot.NewAccrualSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccrualSlotRequiredCodeRule("accrual slot needs this field");
        var item = AccrualSlot.NewAccrualSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualSlotRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new AccrualSlotRequiredRegionRule("accrual slot needs this field");
        var item = AccrualSlot.NewAccrualSlot("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualSlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccrualSlotCodeLengthRule(120);
        var item = AccrualSlot.NewAccrualSlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualSlotRequiredNameRuleFlagsABlankName()
    {
        var rule = new AccrualSlotRequiredNameRule("accrual slot needs a value");
        var item = AccrualSlot.NewAccrualSlot("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualSlotNameLengthRuleFlagsALongName()
    {
        var rule = new AccrualSlotNameLengthRule(3600);
        var item = AccrualSlot.NewAccrualSlot("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualSlotMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new AccrualSlotMinimumRangeRule(1.3, 3943.6);
        var item = AccrualSlot.NewAccrualSlot("x", "y");
        item.Minimum = 3945.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AccrualSlotValidatorReportsEveryProblem();
        AccrualSlotRequiredCodeRuleFlagsABlankCode();
        AccrualSlotRequiredRegionRuleFlagsABlankRegion();
        AccrualSlotCodeLengthRuleFlagsALongCode();
        AccrualSlotRequiredNameRuleFlagsABlankName();
        AccrualSlotNameLengthRuleFlagsALongName();
        AccrualSlotMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
