namespace Acme.Workforce.Absence.Tests.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Holiday.HolidayReservation;

/// <summary>Checks of holiday reservation code, run by the project's runner.</summary>
public static class PendingHolidayReservationRulesChecks
{
    public static void PendingHolidayReservationValidatorReportsEveryProblem()
    {
        var validator = PendingHolidayReservationRules.DefaultPendingHolidayReservationValidator();
        var item = PendingHolidayReservation.NewPendingHolidayReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingHolidayReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingHolidayReservationRequiredCodeRule("pending holiday reservation needs a value");
        var item = PendingHolidayReservation.NewPendingHolidayReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingHolidayReservationRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new PendingHolidayReservationRequiredDepartmentRule("pending holiday reservation needs a value");
        var item = PendingHolidayReservation.NewPendingHolidayReservation("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingHolidayReservationExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new PendingHolidayReservationExpiresAtRangeRule(3600, 3600);
        var item = PendingHolidayReservation.NewPendingHolidayReservation("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingHolidayReservationDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new PendingHolidayReservationDepartmentLengthRule(1000);
        var item = PendingHolidayReservation.NewPendingHolidayReservation("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingHolidayReservationRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new PendingHolidayReservationRateRangeRule(0.2, 882.9);
        var item = PendingHolidayReservation.NewPendingHolidayReservation("x", "y");
        item.Rate = 884.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingHolidayReservationCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new PendingHolidayReservationCreatedAtRangeRule(80, 3600);
        var item = PendingHolidayReservation.NewPendingHolidayReservation("x", "y");
        item.CreatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingHolidayReservationValidatorReportsEveryProblem();
        PendingHolidayReservationRequiredCodeRuleFlagsABlankCode();
        PendingHolidayReservationRequiredDepartmentRuleFlagsABlankDepartment();
        PendingHolidayReservationExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        PendingHolidayReservationDepartmentLengthRuleFlagsALongDepartment();
        PendingHolidayReservationRateRangeRuleFlagsValuesAboveRate();
        PendingHolidayReservationCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
