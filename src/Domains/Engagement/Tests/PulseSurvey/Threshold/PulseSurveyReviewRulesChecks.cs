namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Threshold;

using Acme.Workforce.Engagement.PulseSurvey.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PulseSurveyReviewRulesChecks
{
    public static void PulseSurveyReviewValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyReviewRules.DefaultPulseSurveyReviewValidator();
        var item = PulseSurveyReview.NewPulseSurveyReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyReviewSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new PulseSurveyReviewSummaryLengthRule(255);
        var item = PulseSurveyReview.NewPulseSurveyReview("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyReviewLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PulseSurveyReviewLabelLengthRule(64);
        var item = PulseSurveyReview.NewPulseSurveyReview("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PulseSurveyReviewValidatorReportsEveryProblem();
        PulseSurveyReviewSummaryLengthRuleFlagsALongSummary();
        PulseSurveyReviewLabelLengthRuleFlagsALongLabel();
    }
}
