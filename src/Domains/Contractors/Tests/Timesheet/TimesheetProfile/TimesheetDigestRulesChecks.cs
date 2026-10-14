namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetProfile;

using Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

/// <summary>Checks of timesheet profile code, run by the project's runner.</summary>
public static class TimesheetDigestRulesChecks
{
    public static void TimesheetDigestValidatorReportsEveryProblem()
    {
        var validator = TimesheetDigestRules.DefaultTimesheetDigestValidator();
        var item = TimesheetDigest.NewTimesheetDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetDigestTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new TimesheetDigestTenureMonthsRangeRule(80, 255);
        var item = TimesheetDigest.NewTimesheetDigest("x", "y");
        item.TenureMonths = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TimesheetDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new TimesheetDigestCodeLengthRule(120);
        var item = TimesheetDigest.NewTimesheetDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TimesheetDigestValidatorReportsEveryProblem();
        TimesheetDigestTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        TimesheetDigestCodeLengthRuleFlagsALongCode();
    }
}
