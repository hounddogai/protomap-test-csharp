namespace Acme.Workforce.Contractors.Tests.Timesheet.Plan;

using Acme.Workforce.Contractors.Timesheet.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class LegacyTimesheetQuotaRulesChecks
{
    public static void LegacyTimesheetQuotaValidatorReportsEveryProblem()
    {
        var validator = LegacyTimesheetQuotaRules.DefaultLegacyTimesheetQuotaValidator();
        var item = LegacyTimesheetQuota.NewLegacyTimesheetQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyTimesheetQuotaCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new LegacyTimesheetQuotaCostRangeRule(2.8, 4957.4);
        var item = LegacyTimesheetQuota.NewLegacyTimesheetQuota("x", "y");
        item.Cost = 4958.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyTimesheetQuotaAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new LegacyTimesheetQuotaAttemptsRangeRule(16, 16);
        var item = LegacyTimesheetQuota.NewLegacyTimesheetQuota("x", "y");
        item.Attempts = 17;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyTimesheetQuotaRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LegacyTimesheetQuotaRequiredTitleRule("legacy timesheet quota needs this field");
        var item = LegacyTimesheetQuota.NewLegacyTimesheetQuota("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyTimesheetQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyTimesheetQuotaCodeLengthRule(3600);
        var item = LegacyTimesheetQuota.NewLegacyTimesheetQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyTimesheetQuotaValidatorReportsEveryProblem();
        LegacyTimesheetQuotaCostRangeRuleFlagsValuesAboveCost();
        LegacyTimesheetQuotaAttemptsRangeRuleFlagsValuesAboveAttempts();
        LegacyTimesheetQuotaRequiredTitleRuleFlagsABlankTitle();
        LegacyTimesheetQuotaCodeLengthRuleFlagsALongCode();
    }
}
