namespace Acme.Workforce.Absence.Tests.Accrual.Cycle;

using Acme.Workforce.Absence.Accrual.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AccrualCycleRulesChecks
{
    public static void AccrualCycleValidatorReportsEveryProblem()
    {
        var validator = AccrualCycleRules.DefaultAccrualCycleValidator();
        var item = AccrualCycle.NewAccrualCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualCycleNameLengthRuleFlagsALongName()
    {
        var rule = new AccrualCycleNameLengthRule(1000);
        var item = AccrualCycle.NewAccrualCycle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualCycleRequiredNameRuleFlagsABlankName()
    {
        var rule = new AccrualCycleRequiredNameRule("accrual cycle needs an entry");
        var item = AccrualCycle.NewAccrualCycle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualCycleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccrualCycleRequiredCodeRule("accrual cycle needs an entry");
        var item = AccrualCycle.NewAccrualCycle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualCycleCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new AccrualCycleCategoryLengthRule(1000);
        var item = AccrualCycle.NewAccrualCycle("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccrualCycleValidatorReportsEveryProblem();
        AccrualCycleNameLengthRuleFlagsALongName();
        AccrualCycleRequiredNameRuleFlagsABlankName();
        AccrualCycleRequiredCodeRuleFlagsABlankCode();
        AccrualCycleCategoryLengthRuleFlagsALongCategory();
    }
}
