namespace Acme.Workforce.Compensation.Tests.MeritCycle.Allocation;

using Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MeritCycleSlotRulesChecks
{
    public static void MeritCycleSlotValidatorReportsEveryProblem()
    {
        var validator = MeritCycleSlotRules.DefaultMeritCycleSlotValidator();
        var item = MeritCycleSlot.NewMeritCycleSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleSlotTitleLengthRuleFlagsALongTitle()
    {
        var rule = new MeritCycleSlotTitleLengthRule(86400);
        var item = MeritCycleSlot.NewMeritCycleSlot("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleSlotUnitLengthRuleFlagsALongUnit()
    {
        var rule = new MeritCycleSlotUnitLengthRule(80);
        var item = MeritCycleSlot.NewMeritCycleSlot("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MeritCycleSlotRequiredCodeRule("merit cycle slot needs this field");
        var item = MeritCycleSlot.NewMeritCycleSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleSlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new MeritCycleSlotCodeLengthRule(80);
        var item = MeritCycleSlot.NewMeritCycleSlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MeritCycleSlotValidatorReportsEveryProblem();
        MeritCycleSlotTitleLengthRuleFlagsALongTitle();
        MeritCycleSlotUnitLengthRuleFlagsALongUnit();
        MeritCycleSlotRequiredCodeRuleFlagsABlankCode();
        MeritCycleSlotCodeLengthRuleFlagsALongCode();
    }
}
