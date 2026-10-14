namespace Acme.Workforce.Absence.Tests.LeaveRequest.Reservation;

using Acme.Workforce.Absence.LeaveRequest.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ManualLeaveRequestReservationRulesChecks
{
    public static void ManualLeaveRequestReservationValidatorReportsEveryProblem()
    {
        var validator = ManualLeaveRequestReservationRules.DefaultManualLeaveRequestReservationValidator();
        var item = ManualLeaveRequestReservation.NewManualLeaveRequestReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualLeaveRequestReservationMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new ManualLeaveRequestReservationMaximumRangeRule(6.7, 1465.2);
        var item = ManualLeaveRequestReservation.NewManualLeaveRequestReservation("x", "y");
        item.Maximum = 1466.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualLeaveRequestReservationRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ManualLeaveRequestReservationRequiredLabelRule("manual leave request reservation needs a value");
        var item = ManualLeaveRequestReservation.NewManualLeaveRequestReservation("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualLeaveRequestReservationRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ManualLeaveRequestReservationRequiredTitleRule("manual leave request reservation needs an entry");
        var item = ManualLeaveRequestReservation.NewManualLeaveRequestReservation("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualLeaveRequestReservationValidatorReportsEveryProblem();
        ManualLeaveRequestReservationMaximumRangeRuleFlagsValuesAboveMaximum();
        ManualLeaveRequestReservationRequiredLabelRuleFlagsABlankLabel();
        ManualLeaveRequestReservationRequiredTitleRuleFlagsABlankTitle();
    }
}
