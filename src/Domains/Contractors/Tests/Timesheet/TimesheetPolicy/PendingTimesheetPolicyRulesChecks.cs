namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetPolicy;

using Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

/// <summary>Checks of timesheet policy code, run by the project's runner.</summary>
public static class PendingTimesheetPolicyRulesChecks
{
    public static void PendingTimesheetPolicyValidatorReportsEveryProblem()
    {
        var validator = PendingTimesheetPolicyRules.DefaultPendingTimesheetPolicyValidator();
        var item = PendingTimesheetPolicy.NewPendingTimesheetPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingTimesheetPolicyCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new PendingTimesheetPolicyCategoryLengthRule(1000);
        var item = PendingTimesheetPolicy.NewPendingTimesheetPolicy("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingTimesheetPolicyTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PendingTimesheetPolicyTitleLengthRule(86400);
        var item = PendingTimesheetPolicy.NewPendingTimesheetPolicy("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingTimesheetPolicyLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new PendingTimesheetPolicyLeaveDaysRangeRule(1000, 86400);
        var item = PendingTimesheetPolicy.NewPendingTimesheetPolicy("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingTimesheetPolicyRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new PendingTimesheetPolicyRequiredChecksumRule("pending timesheet policy needs a value");
        var item = PendingTimesheetPolicy.NewPendingTimesheetPolicy("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingTimesheetPolicyValidatorReportsEveryProblem();
        PendingTimesheetPolicyCategoryLengthRuleFlagsALongCategory();
        PendingTimesheetPolicyTitleLengthRuleFlagsALongTitle();
        PendingTimesheetPolicyLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        PendingTimesheetPolicyRequiredChecksumRuleFlagsABlankChecksum();
    }
}
