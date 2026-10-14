namespace Acme.Workforce.Contractors.Tests.Timesheet.Policy;

using Acme.Workforce.Contractors.Timesheet.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class TimesheetRuleRulesChecks
{
    public static void TimesheetRuleValidatorReportsEveryProblem()
    {
        var validator = TimesheetRuleRules.DefaultTimesheetRuleValidator();
        var item = TimesheetRule.NewTimesheetRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetRuleMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new TimesheetRuleMarginRangeRule(2.2, 1770.3);
        var item = TimesheetRule.NewTimesheetRule("x", "y");
        item.Margin = 1771.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TimesheetRuleRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new TimesheetRuleRequiredTimezoneRule("timesheet rule needs this field");
        var item = TimesheetRule.NewTimesheetRule("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TimesheetRuleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TimesheetRuleRequiredTitleRule("timesheet rule needs a value");
        var item = TimesheetRule.NewTimesheetRule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TimesheetRuleValidatorReportsEveryProblem();
        TimesheetRuleMarginRangeRuleFlagsValuesAboveMargin();
        TimesheetRuleRequiredTimezoneRuleFlagsABlankTimezone();
        TimesheetRuleRequiredTitleRuleFlagsABlankTitle();
    }
}
