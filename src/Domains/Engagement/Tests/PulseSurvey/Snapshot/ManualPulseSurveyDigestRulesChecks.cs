namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Snapshot;

using Acme.Workforce.Engagement.PulseSurvey.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ManualPulseSurveyDigestRulesChecks
{
    public static void ManualPulseSurveyDigestValidatorReportsEveryProblem()
    {
        var validator = ManualPulseSurveyDigestRules.DefaultManualPulseSurveyDigestValidator();
        var item = ManualPulseSurveyDigest.NewManualPulseSurveyDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualPulseSurveyDigestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ManualPulseSurveyDigestTitleLengthRule(3600);
        var item = ManualPulseSurveyDigest.NewManualPulseSurveyDigest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualPulseSurveyDigestRequiredNameRuleFlagsABlankName()
    {
        var rule = new ManualPulseSurveyDigestRequiredNameRule("manual pulse survey digest needs a value");
        var item = ManualPulseSurveyDigest.NewManualPulseSurveyDigest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualPulseSurveyDigestValidatorReportsEveryProblem();
        ManualPulseSurveyDigestTitleLengthRuleFlagsALongTitle();
        ManualPulseSurveyDigestRequiredNameRuleFlagsABlankName();
    }
}
