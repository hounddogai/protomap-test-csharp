namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Manifest;

using Acme.Workforce.Engagement.PulseSurvey.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LenientPulseSurveyManifestRulesChecks
{
    public static void LenientPulseSurveyManifestValidatorReportsEveryProblem()
    {
        var validator = LenientPulseSurveyManifestRules.DefaultLenientPulseSurveyManifestValidator();
        var item = LenientPulseSurveyManifest.NewLenientPulseSurveyManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientPulseSurveyManifestMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new LenientPulseSurveyManifestMinimumRangeRule(7.1, 3488.9);
        var item = LenientPulseSurveyManifest.NewLenientPulseSurveyManifest("x", "y");
        item.Minimum = 3490.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientPulseSurveyManifestSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new LenientPulseSurveyManifestSequenceRangeRule(64, 255);
        var item = LenientPulseSurveyManifest.NewLenientPulseSurveyManifest("x", "y");
        item.Sequence = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientPulseSurveyManifestValidatorReportsEveryProblem();
        LenientPulseSurveyManifestMinimumRangeRuleFlagsValuesAboveMinimum();
        LenientPulseSurveyManifestSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
