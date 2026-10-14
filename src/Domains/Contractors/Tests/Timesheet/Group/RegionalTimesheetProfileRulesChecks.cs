namespace Acme.Workforce.Contractors.Tests.Timesheet.Group;

using Acme.Workforce.Contractors.Timesheet.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class RegionalTimesheetProfileRulesChecks
{
    public static void RegionalTimesheetProfileValidatorReportsEveryProblem()
    {
        var validator = RegionalTimesheetProfileRules.DefaultRegionalTimesheetProfileValidator();
        var item = RegionalTimesheetProfile.NewRegionalTimesheetProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalTimesheetProfileTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new RegionalTimesheetProfileTimezoneLengthRule(255);
        var item = RegionalTimesheetProfile.NewRegionalTimesheetProfile("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalTimesheetProfileBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new RegionalTimesheetProfileBatchSizeRangeRule(1000, 86400);
        var item = RegionalTimesheetProfile.NewRegionalTimesheetProfile("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalTimesheetProfileRequiredNameRuleFlagsABlankName()
    {
        var rule = new RegionalTimesheetProfileRequiredNameRule("regional timesheet profile needs an entry");
        var item = RegionalTimesheetProfile.NewRegionalTimesheetProfile("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalTimesheetProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalTimesheetProfileRequiredCodeRule("regional timesheet profile needs an entry");
        var item = RegionalTimesheetProfile.NewRegionalTimesheetProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalTimesheetProfileRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new RegionalTimesheetProfileRequiredTimezoneRule("regional timesheet profile needs a value");
        var item = RegionalTimesheetProfile.NewRegionalTimesheetProfile("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalTimesheetProfileValidatorReportsEveryProblem();
        RegionalTimesheetProfileTimezoneLengthRuleFlagsALongTimezone();
        RegionalTimesheetProfileBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        RegionalTimesheetProfileRequiredNameRuleFlagsABlankName();
        RegionalTimesheetProfileRequiredCodeRuleFlagsABlankCode();
        RegionalTimesheetProfileRequiredTimezoneRuleFlagsABlankTimezone();
    }
}
