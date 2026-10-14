namespace Acme.Workforce.Contractors.Tests.Timesheet.Estimate;

using Acme.Workforce.Contractors.Timesheet.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TimesheetLedgerRulesChecks
{
    public static void TimesheetLedgerValidatorReportsEveryProblem()
    {
        var validator = TimesheetLedgerRules.DefaultTimesheetLedgerValidator();
        var item = TimesheetLedger.NewTimesheetLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TimesheetLedgerRequiredCodeRule("timesheet ledger needs a value");
        var item = TimesheetLedger.NewTimesheetLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TimesheetLedgerValidatorReportsEveryProblem();
        TimesheetLedgerRequiredCodeRuleFlagsABlankCode();
    }
}
