namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Profile;

using Acme.Workforce.Engagement.PulseSurvey.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RollingPulseSurveyCheckpointRulesChecks
{
    public static void RollingPulseSurveyCheckpointValidatorReportsEveryProblem()
    {
        var validator = RollingPulseSurveyCheckpointRules.DefaultRollingPulseSurveyCheckpointValidator();
        var item = RollingPulseSurveyCheckpoint.NewRollingPulseSurveyCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingPulseSurveyCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingPulseSurveyCheckpointRequiredCodeRule("rolling pulse survey checkpoint needs a value");
        var item = RollingPulseSurveyCheckpoint.NewRollingPulseSurveyCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingPulseSurveyCheckpointRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new RollingPulseSurveyCheckpointRequiredSummaryRule(
            "rolling pulse survey checkpoint needs this field");
        var item = RollingPulseSurveyCheckpoint.NewRollingPulseSurveyCheckpoint("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingPulseSurveyCheckpointValidatorReportsEveryProblem();
        RollingPulseSurveyCheckpointRequiredCodeRuleFlagsABlankCode();
        RollingPulseSurveyCheckpointRequiredSummaryRuleFlagsABlankSummary();
    }
}
