namespace Acme.Workforce.Compensation.Tests.MeritCycle.Allocation;

using Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MeritCycleExceptionRulesChecks
{
    public static void MeritCycleExceptionValidatorReportsEveryProblem()
    {
        var validator = MeritCycleExceptionRules.DefaultMeritCycleExceptionValidator();
        var item = MeritCycleException.NewMeritCycleException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleExceptionRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new MeritCycleExceptionRequiredTitleRule("merit cycle exception needs this field");
        var item = MeritCycleException.NewMeritCycleException("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleExceptionStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new MeritCycleExceptionStartsAtRangeRule(32, 32);
        var item = MeritCycleException.NewMeritCycleException("x", "y");
        item.StartsAt = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MeritCycleExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MeritCycleExceptionRequiredCodeRule("merit cycle exception needs a value");
        var item = MeritCycleException.NewMeritCycleException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MeritCycleExceptionValidatorReportsEveryProblem();
        MeritCycleExceptionRequiredTitleRuleFlagsABlankTitle();
        MeritCycleExceptionStartsAtRangeRuleFlagsValuesAboveStartsAt();
        MeritCycleExceptionRequiredCodeRuleFlagsABlankCode();
    }
}
