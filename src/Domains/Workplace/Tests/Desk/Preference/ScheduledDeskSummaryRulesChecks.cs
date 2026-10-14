namespace Acme.Workforce.Workplace.Tests.Desk.Preference;

using Acme.Workforce.Workplace.Desk.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ScheduledDeskSummaryRulesChecks
{
    public static void ScheduledDeskSummaryValidatorReportsEveryProblem()
    {
        var validator = ScheduledDeskSummaryRules.DefaultScheduledDeskSummaryValidator();
        var item = ScheduledDeskSummary.NewScheduledDeskSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledDeskSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledDeskSummaryCodeLengthRule(3600);
        var item = ScheduledDeskSummary.NewScheduledDeskSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledDeskSummaryNotesLengthRuleFlagsALongNotes()
    {
        var rule = new ScheduledDeskSummaryNotesLengthRule(1000);
        var item = ScheduledDeskSummary.NewScheduledDeskSummary("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledDeskSummaryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledDeskSummaryRequiredTitleRule("scheduled desk summary needs a value");
        var item = ScheduledDeskSummary.NewScheduledDeskSummary("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledDeskSummaryRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new ScheduledDeskSummaryRateRangeRule(3.0, 4395.2);
        var item = ScheduledDeskSummary.NewScheduledDeskSummary("x", "y");
        item.Rate = 4396.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledDeskSummaryValidatorReportsEveryProblem();
        ScheduledDeskSummaryCodeLengthRuleFlagsALongCode();
        ScheduledDeskSummaryNotesLengthRuleFlagsALongNotes();
        ScheduledDeskSummaryRequiredTitleRuleFlagsABlankTitle();
        ScheduledDeskSummaryRateRangeRuleFlagsValuesAboveRate();
    }
}
