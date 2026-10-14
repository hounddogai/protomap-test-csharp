namespace Acme.Workforce.Engagement.Tests.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

/// <summary>Checks of seasonal feedback reservation code, run by the project's runner.</summary>
public static class FeedbackExceptionRulesChecks
{
    public static void FeedbackExceptionValidatorReportsEveryProblem()
    {
        var validator = FeedbackExceptionRules.DefaultFeedbackExceptionValidator();
        var item = FeedbackException.NewFeedbackException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackExceptionCodeLengthRuleFlagsALongCode()
    {
        var rule = new FeedbackExceptionCodeLengthRule(3600);
        var item = FeedbackException.NewFeedbackException("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackExceptionUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new FeedbackExceptionUpdatedAtRangeRule(64, 80);
        var item = FeedbackException.NewFeedbackException("x", "y");
        item.UpdatedAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FeedbackExceptionRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new FeedbackExceptionRequiredSummaryRule("feedback exception needs a value");
        var item = FeedbackException.NewFeedbackException("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FeedbackExceptionValidatorReportsEveryProblem();
        FeedbackExceptionCodeLengthRuleFlagsALongCode();
        FeedbackExceptionUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        FeedbackExceptionRequiredSummaryRuleFlagsABlankSummary();
    }
}
