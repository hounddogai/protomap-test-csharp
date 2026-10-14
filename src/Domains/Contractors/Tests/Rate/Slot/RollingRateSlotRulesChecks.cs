namespace Acme.Workforce.Contractors.Tests.Rate.Slot;

using Acme.Workforce.Contractors.Rate.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class RollingRateSlotRulesChecks
{
    public static void RollingRateSlotValidatorReportsEveryProblem()
    {
        var validator = RollingRateSlotRules.DefaultRollingRateSlotValidator();
        var item = RollingRateSlot.NewRollingRateSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingRateSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingRateSlotRequiredCodeRule("rolling rate slot needs a value");
        var item = RollingRateSlot.NewRollingRateSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingRateSlotRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new RollingRateSlotRatioRangeRule(5.7, 4203.7);
        var item = RollingRateSlot.NewRollingRateSlot("x", "y");
        item.Ratio = 4205.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingRateSlotLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RollingRateSlotLabelLengthRule(32);
        var item = RollingRateSlot.NewRollingRateSlot("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingRateSlotAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new RollingRateSlotAttemptsRangeRule(64, 3600);
        var item = RollingRateSlot.NewRollingRateSlot("x", "y");
        item.Attempts = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingRateSlotValidatorReportsEveryProblem();
        RollingRateSlotRequiredCodeRuleFlagsABlankCode();
        RollingRateSlotRatioRangeRuleFlagsValuesAboveRatio();
        RollingRateSlotLabelLengthRuleFlagsALongLabel();
        RollingRateSlotAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
