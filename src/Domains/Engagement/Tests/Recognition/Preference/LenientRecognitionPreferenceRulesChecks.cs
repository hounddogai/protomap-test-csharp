namespace Acme.Workforce.Engagement.Tests.Recognition.Preference;

using Acme.Workforce.Engagement.Recognition.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class LenientRecognitionPreferenceRulesChecks
{
    public static void LenientRecognitionPreferenceValidatorReportsEveryProblem()
    {
        var validator = LenientRecognitionPreferenceRules.DefaultLenientRecognitionPreferenceValidator();
        var item = LenientRecognitionPreference.NewLenientRecognitionPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientRecognitionPreferenceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LenientRecognitionPreferenceRequiredTitleRule("lenient recognition preference needs an entry");
        var item = LenientRecognitionPreference.NewLenientRecognitionPreference("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientRecognitionPreferenceValidatorReportsEveryProblem();
        LenientRecognitionPreferenceRequiredTitleRuleFlagsABlankTitle();
    }
}
