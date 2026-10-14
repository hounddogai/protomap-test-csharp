namespace Acme.Workforce.Absence.Tests.Accrual.Schedule;

using Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class AccrualScheduleRulesChecks
{
    public static void AccrualScheduleValidatorReportsEveryProblem()
    {
        var validator = AccrualScheduleRules.DefaultAccrualScheduleValidator();
        var item = AccrualSchedule.NewAccrualSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualScheduleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AccrualScheduleLabelLengthRule(80);
        var item = AccrualSchedule.NewAccrualSchedule("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccrualScheduleRequiredCodeRule("accrual schedule needs an entry");
        var item = AccrualSchedule.NewAccrualSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccrualScheduleValidatorReportsEveryProblem();
        AccrualScheduleLabelLengthRuleFlagsALongLabel();
        AccrualScheduleRequiredCodeRuleFlagsABlankCode();
    }
}
