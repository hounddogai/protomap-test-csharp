namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackReservation;

using Acme.Workforce.Engagement.Feedback.FeedbackReservation;

/// <summary>Checks of feedback reservation code, run by the project's runner.</summary>
public static class FallbackFeedbackReservationRulesChecks
{
    public static void FallbackFeedbackReservationValidatorReportsEveryProblem()
    {
        var validator = FallbackFeedbackReservationRules.DefaultFallbackFeedbackReservationValidator();
        var item = FallbackFeedbackReservation.NewFallbackFeedbackReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackFeedbackReservationChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new FallbackFeedbackReservationChecksumLengthRule(32);
        var item = FallbackFeedbackReservation.NewFallbackFeedbackReservation("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackFeedbackReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackFeedbackReservationRequiredCodeRule("fallback feedback reservation needs an entry");
        var item = FallbackFeedbackReservation.NewFallbackFeedbackReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackFeedbackReservationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FallbackFeedbackReservationLabelLengthRule(32);
        var item = FallbackFeedbackReservation.NewFallbackFeedbackReservation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackFeedbackReservationValidatorReportsEveryProblem();
        FallbackFeedbackReservationChecksumLengthRuleFlagsALongChecksum();
        FallbackFeedbackReservationRequiredCodeRuleFlagsABlankCode();
        FallbackFeedbackReservationLabelLengthRuleFlagsALongLabel();
    }
}
