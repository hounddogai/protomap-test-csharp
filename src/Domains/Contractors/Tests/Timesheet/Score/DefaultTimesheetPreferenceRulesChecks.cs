namespace Acme.Workforce.Contractors.Tests.Timesheet.Score;

using Acme.Workforce.Contractors.Timesheet.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class DefaultTimesheetPreferenceRulesChecks
{
    public static void DefaultTimesheetPreferenceValidatorReportsEveryProblem()
    {
        var validator = DefaultTimesheetPreferenceRules.DefaultDefaultTimesheetPreferenceValidator();
        var item = DefaultTimesheetPreference.NewDefaultTimesheetPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultTimesheetPreferenceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DefaultTimesheetPreferenceTitleLengthRule(80);
        var item = DefaultTimesheetPreference.NewDefaultTimesheetPreference("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultTimesheetPreferenceScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new DefaultTimesheetPreferenceScoreRangeRule(9.1, 1165.2);
        var item = DefaultTimesheetPreference.NewDefaultTimesheetPreference("x", "y");
        item.Score = 1166.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultTimesheetPreferenceValidatorReportsEveryProblem();
        DefaultTimesheetPreferenceTitleLengthRuleFlagsALongTitle();
        DefaultTimesheetPreferenceScoreRangeRuleFlagsValuesAboveScore();
    }
}
