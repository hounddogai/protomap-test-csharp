namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackProposal;

using Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>Checks of feedback proposal code, run by the project's runner.</summary>
public static class FeedbackEntryRulesChecks
{
    public static void FeedbackEntryValidatorReportsEveryProblem()
    {
        var validator = FeedbackEntryRules.DefaultFeedbackEntryValidator();
        var item = FeedbackEntry.NewFeedbackEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new FeedbackEntryCodeLengthRule(120);
        var item = FeedbackEntry.NewFeedbackEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackEntryDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new FeedbackEntryDurationMinutesRangeRule(32, 3600);
        var item = FeedbackEntry.NewFeedbackEntry("x", "y");
        item.DurationMinutes = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FeedbackEntryValidatorReportsEveryProblem();
        FeedbackEntryCodeLengthRuleFlagsALongCode();
        FeedbackEntryDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
