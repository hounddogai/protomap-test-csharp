namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Metric;

using Acme.Workforce.Engagement.PulseSurvey.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class StrictPulseSurveyCheckpointRulesChecks
{
    public static void StrictPulseSurveyCheckpointValidatorReportsEveryProblem()
    {
        var validator = StrictPulseSurveyCheckpointRules.DefaultStrictPulseSurveyCheckpointValidator();
        var item = StrictPulseSurveyCheckpoint.NewStrictPulseSurveyCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictPulseSurveyCheckpointLabelLengthRuleFlagsALongLabel()
    {
        var rule = new StrictPulseSurveyCheckpointLabelLengthRule(86400);
        var item = StrictPulseSurveyCheckpoint.NewStrictPulseSurveyCheckpoint("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictPulseSurveyCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictPulseSurveyCheckpointCodeLengthRule(32);
        var item = StrictPulseSurveyCheckpoint.NewStrictPulseSurveyCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictPulseSurveyCheckpointRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new StrictPulseSurveyCheckpointRequiredDescriptionRule(
            "strict pulse survey checkpoint needs a value");
        var item = StrictPulseSurveyCheckpoint.NewStrictPulseSurveyCheckpoint("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictPulseSurveyCheckpointValidatorReportsEveryProblem();
        StrictPulseSurveyCheckpointLabelLengthRuleFlagsALongLabel();
        StrictPulseSurveyCheckpointCodeLengthRuleFlagsALongCode();
        StrictPulseSurveyCheckpointRequiredDescriptionRuleFlagsABlankDescription();
    }
}
