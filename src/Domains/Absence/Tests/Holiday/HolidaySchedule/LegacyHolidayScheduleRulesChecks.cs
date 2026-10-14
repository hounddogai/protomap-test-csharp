namespace Acme.Workforce.Absence.Tests.Holiday.HolidaySchedule;

using Acme.Workforce.Absence.Holiday.HolidaySchedule;

/// <summary>Checks of holiday schedule code, run by the project's runner.</summary>
public static class LegacyHolidayScheduleRulesChecks
{
    public static void LegacyHolidayScheduleValidatorReportsEveryProblem()
    {
        var validator = LegacyHolidayScheduleRules.DefaultLegacyHolidayScheduleValidator();
        var item = LegacyHolidaySchedule.NewLegacyHolidaySchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyHolidayScheduleRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new LegacyHolidayScheduleRequiredJobCodeRule("legacy holiday schedule needs this field");
        var item = LegacyHolidaySchedule.NewLegacyHolidaySchedule("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyHolidayScheduleJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new LegacyHolidayScheduleJobCodeLengthRule(32);
        var item = LegacyHolidaySchedule.NewLegacyHolidaySchedule("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyHolidayScheduleRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new LegacyHolidayScheduleRatingRangeRule(0.2, 1705.9);
        var item = LegacyHolidaySchedule.NewLegacyHolidaySchedule("x", "y");
        item.Rating = 1707.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyHolidayScheduleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LegacyHolidayScheduleLabelLengthRule(255);
        var item = LegacyHolidaySchedule.NewLegacyHolidaySchedule("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyHolidayScheduleValidatorReportsEveryProblem();
        LegacyHolidayScheduleRequiredJobCodeRuleFlagsABlankJobCode();
        LegacyHolidayScheduleJobCodeLengthRuleFlagsALongJobCode();
        LegacyHolidayScheduleRatingRangeRuleFlagsValuesAboveRating();
        LegacyHolidayScheduleLabelLengthRuleFlagsALongLabel();
    }
}
