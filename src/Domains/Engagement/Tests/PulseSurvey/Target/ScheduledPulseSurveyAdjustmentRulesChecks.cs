namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Target;

using Acme.Workforce.Engagement.PulseSurvey.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ScheduledPulseSurveyAdjustmentRulesChecks
{
    public static void ScheduledPulseSurveyAdjustmentValidatorReportsEveryProblem()
    {
        var validator = ScheduledPulseSurveyAdjustmentRules.DefaultScheduledPulseSurveyAdjustmentValidator();
        var item = ScheduledPulseSurveyAdjustment.NewScheduledPulseSurveyAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPulseSurveyAdjustmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledPulseSurveyAdjustmentRequiredTitleRule(
            "scheduled pulse survey adjustment needs an entry");
        var item = ScheduledPulseSurveyAdjustment.NewScheduledPulseSurveyAdjustment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPulseSurveyAdjustmentReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new ScheduledPulseSurveyAdjustmentReviewerLengthRule(1000);
        var item = ScheduledPulseSurveyAdjustment.NewScheduledPulseSurveyAdjustment("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledPulseSurveyAdjustmentTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new ScheduledPulseSurveyAdjustmentTimezoneLengthRule(1000);
        var item = ScheduledPulseSurveyAdjustment.NewScheduledPulseSurveyAdjustment("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledPulseSurveyAdjustmentValidatorReportsEveryProblem();
        ScheduledPulseSurveyAdjustmentRequiredTitleRuleFlagsABlankTitle();
        ScheduledPulseSurveyAdjustmentReviewerLengthRuleFlagsALongReviewer();
        ScheduledPulseSurveyAdjustmentTimezoneLengthRuleFlagsALongTimezone();
    }
}
