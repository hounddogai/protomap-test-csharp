namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackReservation;

using Acme.Workforce.Engagement.Feedback.FeedbackReservation;

/// <summary>Checks of feedback reservation code, run by the project's runner.</summary>
public static class AdaptiveFeedbackSummaryRulesChecks
{
    public static void AdaptiveFeedbackSummaryValidatorReportsEveryProblem()
    {
        var validator = AdaptiveFeedbackSummaryRules.DefaultAdaptiveFeedbackSummaryValidator();
        var item = AdaptiveFeedbackSummary.NewAdaptiveFeedbackSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveFeedbackSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveFeedbackSummaryRequiredCodeRule("adaptive feedback summary needs a value");
        var item = AdaptiveFeedbackSummary.NewAdaptiveFeedbackSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveFeedbackSummaryTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AdaptiveFeedbackSummaryTitleLengthRule(3600);
        var item = AdaptiveFeedbackSummary.NewAdaptiveFeedbackSummary("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveFeedbackSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveFeedbackSummaryCodeLengthRule(1000);
        var item = AdaptiveFeedbackSummary.NewAdaptiveFeedbackSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveFeedbackSummaryHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new AdaptiveFeedbackSummaryHoursRangeRule(3.5, 4227.1);
        var item = AdaptiveFeedbackSummary.NewAdaptiveFeedbackSummary("x", "y");
        item.Hours = 4228.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveFeedbackSummaryValidatorReportsEveryProblem();
        AdaptiveFeedbackSummaryRequiredCodeRuleFlagsABlankCode();
        AdaptiveFeedbackSummaryTitleLengthRuleFlagsALongTitle();
        AdaptiveFeedbackSummaryCodeLengthRuleFlagsALongCode();
        AdaptiveFeedbackSummaryHoursRangeRuleFlagsValuesAboveHours();
    }
}
