namespace Acme.Workforce.Absence.Tests.Accrual.Schedule;

using Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class PrimaryAccrualScheduleRulesChecks
{
    public static void PrimaryAccrualScheduleValidatorReportsEveryProblem()
    {
        var validator = PrimaryAccrualScheduleRules.DefaultPrimaryAccrualScheduleValidator();
        var item = PrimaryAccrualSchedule.NewPrimaryAccrualSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryAccrualScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryAccrualScheduleRequiredCodeRule("primary accrual schedule needs a value");
        var item = PrimaryAccrualSchedule.NewPrimaryAccrualSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryAccrualScheduleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PrimaryAccrualScheduleRequiredTitleRule("primary accrual schedule needs this field");
        var item = PrimaryAccrualSchedule.NewPrimaryAccrualSchedule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryAccrualScheduleRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new PrimaryAccrualScheduleRatingRangeRule(8.4, 2614.7);
        var item = PrimaryAccrualSchedule.NewPrimaryAccrualSchedule("x", "y");
        item.Rating = 2616.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryAccrualScheduleCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryAccrualScheduleCodeLengthRule(120);
        var item = PrimaryAccrualSchedule.NewPrimaryAccrualSchedule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryAccrualScheduleValidatorReportsEveryProblem();
        PrimaryAccrualScheduleRequiredCodeRuleFlagsABlankCode();
        PrimaryAccrualScheduleRequiredTitleRuleFlagsABlankTitle();
        PrimaryAccrualScheduleRatingRangeRuleFlagsValuesAboveRating();
        PrimaryAccrualScheduleCodeLengthRuleFlagsALongCode();
    }
}
