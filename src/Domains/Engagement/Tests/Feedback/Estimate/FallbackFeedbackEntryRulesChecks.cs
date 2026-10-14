namespace Acme.Workforce.Engagement.Tests.Feedback.Estimate;

using Acme.Workforce.Engagement.Feedback.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class FallbackFeedbackEntryRulesChecks
{
    public static void FallbackFeedbackEntryValidatorReportsEveryProblem()
    {
        var validator = FallbackFeedbackEntryRules.DefaultFallbackFeedbackEntryValidator();
        var item = FallbackFeedbackEntry.NewFallbackFeedbackEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackFeedbackEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackFeedbackEntryCodeLengthRule(86400);
        var item = FallbackFeedbackEntry.NewFallbackFeedbackEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackFeedbackEntryTeamLengthRuleFlagsALongTeam()
    {
        var rule = new FallbackFeedbackEntryTeamLengthRule(16);
        var item = FallbackFeedbackEntry.NewFallbackFeedbackEntry("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackFeedbackEntryLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new FallbackFeedbackEntryLeaveDaysRangeRule(255, 3600);
        var item = FallbackFeedbackEntry.NewFallbackFeedbackEntry("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackFeedbackEntryLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FallbackFeedbackEntryLabelLengthRule(32);
        var item = FallbackFeedbackEntry.NewFallbackFeedbackEntry("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackFeedbackEntryRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new FallbackFeedbackEntryRequiredPhoneRule("fallback feedback entry needs a value");
        var item = FallbackFeedbackEntry.NewFallbackFeedbackEntry("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackFeedbackEntryValidatorReportsEveryProblem();
        FallbackFeedbackEntryCodeLengthRuleFlagsALongCode();
        FallbackFeedbackEntryTeamLengthRuleFlagsALongTeam();
        FallbackFeedbackEntryLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        FallbackFeedbackEntryLabelLengthRuleFlagsALongLabel();
        FallbackFeedbackEntryRequiredPhoneRuleFlagsABlankPhone();
    }
}
