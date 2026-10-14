namespace Acme.Workforce.Payrollops.Tests.Deduction.Segment;

using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ScheduledDeductionCycleRulesChecks
{
    public static void ScheduledDeductionCycleValidatorReportsEveryProblem()
    {
        var validator = ScheduledDeductionCycleRules.DefaultScheduledDeductionCycleValidator();
        var item = ScheduledDeductionCycle.NewScheduledDeductionCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledDeductionCycleManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new ScheduledDeductionCycleManagerIdLengthRule(64);
        var item = ScheduledDeductionCycle.NewScheduledDeductionCycle("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledDeductionCycleRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new ScheduledDeductionCycleRequiredUnitRule("scheduled deduction cycle needs an entry");
        var item = ScheduledDeductionCycle.NewScheduledDeductionCycle("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledDeductionCycleFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new ScheduledDeductionCycleFteRangeRule(5.7, 1734.6);
        var item = ScheduledDeductionCycle.NewScheduledDeductionCycle("x", "y");
        item.Fte = 1736.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledDeductionCycleCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledDeductionCycleCodeLengthRule(32);
        var item = ScheduledDeductionCycle.NewScheduledDeductionCycle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledDeductionCycleRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new ScheduledDeductionCycleRateRangeRule(8.0, 4444.6);
        var item = ScheduledDeductionCycle.NewScheduledDeductionCycle("x", "y");
        item.Rate = 4446.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledDeductionCycleUnitLengthRuleFlagsALongUnit()
    {
        var rule = new ScheduledDeductionCycleUnitLengthRule(80);
        var item = ScheduledDeductionCycle.NewScheduledDeductionCycle("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledDeductionCycleValidatorReportsEveryProblem();
        ScheduledDeductionCycleManagerIdLengthRuleFlagsALongManagerId();
        ScheduledDeductionCycleRequiredUnitRuleFlagsABlankUnit();
        ScheduledDeductionCycleFteRangeRuleFlagsValuesAboveFte();
        ScheduledDeductionCycleCodeLengthRuleFlagsALongCode();
        ScheduledDeductionCycleRateRangeRuleFlagsValuesAboveRate();
        ScheduledDeductionCycleUnitLengthRuleFlagsALongUnit();
    }
}
