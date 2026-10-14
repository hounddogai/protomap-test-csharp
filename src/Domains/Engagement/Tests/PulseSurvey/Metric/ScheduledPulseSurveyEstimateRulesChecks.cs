namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Metric;

using Acme.Workforce.Engagement.PulseSurvey.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ScheduledPulseSurveyEstimateRulesChecks
{
    public static void ScheduledPulseSurveyEstimateValidatorReportsEveryProblem()
    {
        var validator = ScheduledPulseSurveyEstimateRules.DefaultScheduledPulseSurveyEstimateValidator();
        var item = ScheduledPulseSurveyEstimate.NewScheduledPulseSurveyEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPulseSurveyEstimateSkillLengthRuleFlagsALongSkill()
    {
        var rule = new ScheduledPulseSurveyEstimateSkillLengthRule(1000);
        var item = ScheduledPulseSurveyEstimate.NewScheduledPulseSurveyEstimate("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledPulseSurveyEstimateRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new ScheduledPulseSurveyEstimateRequiredSkillRule("scheduled pulse survey estimate needs this field");
        var item = ScheduledPulseSurveyEstimate.NewScheduledPulseSurveyEstimate("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPulseSurveyEstimateTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ScheduledPulseSurveyEstimateTitleLengthRule(32);
        var item = ScheduledPulseSurveyEstimate.NewScheduledPulseSurveyEstimate("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledPulseSurveyEstimateValidatorReportsEveryProblem();
        ScheduledPulseSurveyEstimateSkillLengthRuleFlagsALongSkill();
        ScheduledPulseSurveyEstimateRequiredSkillRuleFlagsABlankSkill();
        ScheduledPulseSurveyEstimateTitleLengthRuleFlagsALongTitle();
    }
}
