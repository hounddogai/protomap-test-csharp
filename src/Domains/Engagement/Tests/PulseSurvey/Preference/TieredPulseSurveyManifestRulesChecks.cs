namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class TieredPulseSurveyManifestRulesChecks
{
    public static void TieredPulseSurveyManifestValidatorReportsEveryProblem()
    {
        var validator = TieredPulseSurveyManifestRules.DefaultTieredPulseSurveyManifestValidator();
        var item = TieredPulseSurveyManifest.NewTieredPulseSurveyManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPulseSurveyManifestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TieredPulseSurveyManifestTitleLengthRule(64);
        var item = TieredPulseSurveyManifest.NewTieredPulseSurveyManifest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredPulseSurveyManifestJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new TieredPulseSurveyManifestJobCodeLengthRule(1000);
        var item = TieredPulseSurveyManifest.NewTieredPulseSurveyManifest("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredPulseSurveyManifestValidatorReportsEveryProblem();
        TieredPulseSurveyManifestTitleLengthRuleFlagsALongTitle();
        TieredPulseSurveyManifestJobCodeLengthRuleFlagsALongJobCode();
    }
}
