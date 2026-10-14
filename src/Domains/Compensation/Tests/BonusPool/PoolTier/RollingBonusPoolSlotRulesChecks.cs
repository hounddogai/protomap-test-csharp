namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolTier;

using Acme.Workforce.Compensation.BonusPool.PoolTier;

/// <summary>Checks of pool tier code, run by the project's runner.</summary>
public static class RollingBonusPoolSlotRulesChecks
{
    public static void RollingBonusPoolSlotValidatorReportsEveryProblem()
    {
        var validator = RollingBonusPoolSlotRules.DefaultRollingBonusPoolSlotValidator();
        var item = RollingBonusPoolSlot.NewRollingBonusPoolSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingBonusPoolSlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingBonusPoolSlotCodeLengthRule(32);
        var item = RollingBonusPoolSlot.NewRollingBonusPoolSlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingBonusPoolSlotNameLengthRuleFlagsALongName()
    {
        var rule = new RollingBonusPoolSlotNameLengthRule(16);
        var item = RollingBonusPoolSlot.NewRollingBonusPoolSlot("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingBonusPoolSlotRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new RollingBonusPoolSlotRequiredSourceRule("rolling bonus pool slot needs this field");
        var item = RollingBonusPoolSlot.NewRollingBonusPoolSlot("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingBonusPoolSlotLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new RollingBonusPoolSlotLeaveDaysRangeRule(32, 1000);
        var item = RollingBonusPoolSlot.NewRollingBonusPoolSlot("x", "y");
        item.LeaveDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingBonusPoolSlotRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new RollingBonusPoolSlotRetriesRangeRule(120, 120);
        var item = RollingBonusPoolSlot.NewRollingBonusPoolSlot("x", "y");
        item.Retries = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingBonusPoolSlotValidatorReportsEveryProblem();
        RollingBonusPoolSlotCodeLengthRuleFlagsALongCode();
        RollingBonusPoolSlotNameLengthRuleFlagsALongName();
        RollingBonusPoolSlotRequiredSourceRuleFlagsABlankSource();
        RollingBonusPoolSlotLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        RollingBonusPoolSlotRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
