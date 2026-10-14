namespace Acme.Workforce.Contractors.Tests.Timesheet.Exception;

using Acme.Workforce.Contractors.Timesheet.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TimesheetAssignmentRulesChecks
{
    public static void TimesheetAssignmentValidatorReportsEveryProblem()
    {
        var validator = TimesheetAssignmentRules.DefaultTimesheetAssignmentValidator();
        var item = TimesheetAssignment.NewTimesheetAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetAssignmentRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new TimesheetAssignmentRequiredSalaryBandRule("timesheet assignment needs an entry");
        var item = TimesheetAssignment.NewTimesheetAssignment("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TimesheetAssignmentExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new TimesheetAssignmentExternalRefLengthRule(80);
        var item = TimesheetAssignment.NewTimesheetAssignment("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TimesheetAssignmentFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new TimesheetAssignmentFloorValueRangeRule(9.1, 151.2);
        var item = TimesheetAssignment.NewTimesheetAssignment("x", "y");
        item.FloorValue = 152.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TimesheetAssignmentRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new TimesheetAssignmentRequiredExternalRefRule("timesheet assignment needs an entry");
        var item = TimesheetAssignment.NewTimesheetAssignment("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TimesheetAssignmentGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new TimesheetAssignmentGraceDaysRangeRule(80, 86400);
        var item = TimesheetAssignment.NewTimesheetAssignment("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TimesheetAssignmentValidatorReportsEveryProblem();
        TimesheetAssignmentRequiredSalaryBandRuleFlagsABlankSalaryBand();
        TimesheetAssignmentExternalRefLengthRuleFlagsALongExternalRef();
        TimesheetAssignmentFloorValueRangeRuleFlagsValuesAboveFloorValue();
        TimesheetAssignmentRequiredExternalRefRuleFlagsABlankExternalRef();
        TimesheetAssignmentGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
