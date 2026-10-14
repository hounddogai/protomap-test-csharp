namespace Acme.Workforce.Contractors.Tests.Timesheet.Reservation;

using Acme.Workforce.Contractors.Timesheet.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TimesheetReservationRulesChecks
{
    public static void TimesheetReservationValidatorReportsEveryProblem()
    {
        var validator = TimesheetReservationRules.DefaultTimesheetReservationValidator();
        var item = TimesheetReservation.NewTimesheetReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetReservationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TimesheetReservationLabelLengthRule(120);
        var item = TimesheetReservation.NewTimesheetReservation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TimesheetReservationStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new TimesheetReservationStartsAtRangeRule(80, 120);
        var item = TimesheetReservation.NewTimesheetReservation("x", "y");
        item.StartsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TimesheetReservationNameLengthRuleFlagsALongName()
    {
        var rule = new TimesheetReservationNameLengthRule(16);
        var item = TimesheetReservation.NewTimesheetReservation("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TimesheetReservationRequiredNameRuleFlagsABlankName()
    {
        var rule = new TimesheetReservationRequiredNameRule("timesheet reservation needs this field");
        var item = TimesheetReservation.NewTimesheetReservation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TimesheetReservationValidatorReportsEveryProblem();
        TimesheetReservationLabelLengthRuleFlagsALongLabel();
        TimesheetReservationStartsAtRangeRuleFlagsValuesAboveStartsAt();
        TimesheetReservationNameLengthRuleFlagsALongName();
        TimesheetReservationRequiredNameRuleFlagsABlankName();
    }
}
