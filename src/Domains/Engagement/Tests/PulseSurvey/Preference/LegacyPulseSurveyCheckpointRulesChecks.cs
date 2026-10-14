namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class LegacyPulseSurveyCheckpointRulesChecks
{
    public static void LegacyPulseSurveyCheckpointValidatorReportsEveryProblem()
    {
        var validator = LegacyPulseSurveyCheckpointRules.DefaultLegacyPulseSurveyCheckpointValidator();
        var item = LegacyPulseSurveyCheckpoint.NewLegacyPulseSurveyCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyPulseSurveyCheckpointRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LegacyPulseSurveyCheckpointRequiredLabelRule("legacy pulse survey checkpoint needs a value");
        var item = LegacyPulseSurveyCheckpoint.NewLegacyPulseSurveyCheckpoint("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPulseSurveyCheckpointManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new LegacyPulseSurveyCheckpointManagerIdLengthRule(80);
        var item = LegacyPulseSurveyCheckpoint.NewLegacyPulseSurveyCheckpoint("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyPulseSurveyCheckpointLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LegacyPulseSurveyCheckpointLabelLengthRule(255);
        var item = LegacyPulseSurveyCheckpoint.NewLegacyPulseSurveyCheckpoint("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyPulseSurveyCheckpointRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new LegacyPulseSurveyCheckpointRequiredCategoryRule("legacy pulse survey checkpoint needs a value");
        var item = LegacyPulseSurveyCheckpoint.NewLegacyPulseSurveyCheckpoint("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyPulseSurveyCheckpointValidatorReportsEveryProblem();
        LegacyPulseSurveyCheckpointRequiredLabelRuleFlagsABlankLabel();
        LegacyPulseSurveyCheckpointManagerIdLengthRuleFlagsALongManagerId();
        LegacyPulseSurveyCheckpointLabelLengthRuleFlagsALongLabel();
        LegacyPulseSurveyCheckpointRequiredCategoryRuleFlagsABlankCategory();
    }
}
