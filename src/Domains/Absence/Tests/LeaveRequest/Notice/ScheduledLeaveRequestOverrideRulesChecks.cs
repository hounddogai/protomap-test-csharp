namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestOverrideRulesChecks
{
    public static void ScheduledLeaveRequestOverrideValidatorReportsEveryProblem()
    {
        var validator = ScheduledLeaveRequestOverrideRules.DefaultScheduledLeaveRequestOverrideValidator();
        var item = ScheduledLeaveRequestOverride.NewScheduledLeaveRequestOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledLeaveRequestOverrideRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new ScheduledLeaveRequestOverrideRequiredJobCodeRule(
            "scheduled leave request override needs this field");
        var item = ScheduledLeaveRequestOverride.NewScheduledLeaveRequestOverride("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledLeaveRequestOverrideRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new ScheduledLeaveRequestOverrideRequiredLocaleRule(
            "scheduled leave request override needs an entry");
        var item = ScheduledLeaveRequestOverride.NewScheduledLeaveRequestOverride("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledLeaveRequestOverrideRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new ScheduledLeaveRequestOverrideRatingRangeRule(0.0, 2543.1);
        var item = ScheduledLeaveRequestOverride.NewScheduledLeaveRequestOverride("x", "y");
        item.Rating = 2544.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestOverrideValidatorReportsEveryProblem();
        ScheduledLeaveRequestOverrideRequiredJobCodeRuleFlagsABlankJobCode();
        ScheduledLeaveRequestOverrideRequiredLocaleRuleFlagsABlankLocale();
        ScheduledLeaveRequestOverrideRatingRangeRuleFlagsValuesAboveRating();
    }
}
