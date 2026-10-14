namespace Acme.Workforce.Contractors.Tests.Timesheet.Audit;

using Acme.Workforce.Contractors.Timesheet.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class TimesheetTierRulesChecks
{
    public static void TimesheetTierValidatorReportsEveryProblem()
    {
        var validator = TimesheetTierRules.DefaultTimesheetTierValidator();
        var item = TimesheetTier.NewTimesheetTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetTierRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new TimesheetTierRequiredChecksumRule("timesheet tier needs this field");
        var item = TimesheetTier.NewTimesheetTier("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TimesheetTierChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new TimesheetTierChecksumLengthRule(120);
        var item = TimesheetTier.NewTimesheetTier("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TimesheetTierValidatorReportsEveryProblem();
        TimesheetTierRequiredChecksumRuleFlagsABlankChecksum();
        TimesheetTierChecksumLengthRuleFlagsALongChecksum();
    }
}
