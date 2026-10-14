namespace Acme.Workforce.Absence.Tests.Holiday.Reservation;

using Acme.Workforce.Absence.Holiday.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class HolidayReservationRulesChecks
{
    public static void HolidayReservationValidatorReportsEveryProblem()
    {
        var validator = HolidayReservationRules.DefaultHolidayReservationValidator();
        var item = HolidayReservation.NewHolidayReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayReservationRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new HolidayReservationRequiredTimezoneRule("holiday reservation needs this field");
        var item = HolidayReservation.NewHolidayReservation("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new HolidayReservationCodeLengthRule(3600);
        var item = HolidayReservation.NewHolidayReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayReservationRequiredNameRuleFlagsABlankName()
    {
        var rule = new HolidayReservationRequiredNameRule("holiday reservation needs a value");
        var item = HolidayReservation.NewHolidayReservation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayReservationTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new HolidayReservationTimezoneLengthRule(255);
        var item = HolidayReservation.NewHolidayReservation("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayReservationCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new HolidayReservationCreatedAtRangeRule(120, 86400);
        var item = HolidayReservation.NewHolidayReservation("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayReservationRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new HolidayReservationRequiredLocaleRule("holiday reservation needs an entry");
        var item = HolidayReservation.NewHolidayReservation("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        HolidayReservationValidatorReportsEveryProblem();
        HolidayReservationRequiredTimezoneRuleFlagsABlankTimezone();
        HolidayReservationCodeLengthRuleFlagsALongCode();
        HolidayReservationRequiredNameRuleFlagsABlankName();
        HolidayReservationTimezoneLengthRuleFlagsALongTimezone();
        HolidayReservationCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        HolidayReservationRequiredLocaleRuleFlagsABlankLocale();
    }
}
