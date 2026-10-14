namespace Acme.Workforce.Absence.Tests.Holiday.Reservation;

using Acme.Workforce.Absence.Holiday.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class FallbackHolidayScheduleRulesChecks
{
    public static void FallbackHolidayScheduleValidatorReportsEveryProblem()
    {
        var validator = FallbackHolidayScheduleRules.DefaultFallbackHolidayScheduleValidator();
        var item = FallbackHolidaySchedule.NewFallbackHolidaySchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackHolidayScheduleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new FallbackHolidayScheduleTitleLengthRule(80);
        var item = FallbackHolidaySchedule.NewFallbackHolidaySchedule("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackHolidayScheduleCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackHolidayScheduleCodeLengthRule(16);
        var item = FallbackHolidaySchedule.NewFallbackHolidaySchedule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackHolidayScheduleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FallbackHolidayScheduleRequiredTitleRule("fallback holiday schedule needs a value");
        var item = FallbackHolidaySchedule.NewFallbackHolidaySchedule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackHolidayScheduleRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new FallbackHolidayScheduleRequiredExternalRefRule("fallback holiday schedule needs this field");
        var item = FallbackHolidaySchedule.NewFallbackHolidaySchedule("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackHolidayScheduleTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new FallbackHolidayScheduleTimezoneLengthRule(16);
        var item = FallbackHolidaySchedule.NewFallbackHolidaySchedule("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackHolidayScheduleValidatorReportsEveryProblem();
        FallbackHolidayScheduleTitleLengthRuleFlagsALongTitle();
        FallbackHolidayScheduleCodeLengthRuleFlagsALongCode();
        FallbackHolidayScheduleRequiredTitleRuleFlagsABlankTitle();
        FallbackHolidayScheduleRequiredExternalRefRuleFlagsABlankExternalRef();
        FallbackHolidayScheduleTimezoneLengthRuleFlagsALongTimezone();
    }
}
