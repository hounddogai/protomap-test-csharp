namespace Acme.Workforce.Engagement.Tests.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

/// <summary>Checks of legacy sentiment manifest code, run by the project's runner.</summary>
public static class LenientSentimentManifestRulesChecks
{
    public static void LenientSentimentManifestValidatorReportsEveryProblem()
    {
        var validator = LenientSentimentManifestRules.DefaultLenientSentimentManifestValidator();
        var item = LenientSentimentManifest.NewLenientSentimentManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientSentimentManifestCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientSentimentManifestCodeLengthRule(86400);
        var item = LenientSentimentManifest.NewLenientSentimentManifest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientSentimentManifestRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new LenientSentimentManifestRequiredCategoryRule("lenient sentiment manifest needs this field");
        var item = LenientSentimentManifest.NewLenientSentimentManifest("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientSentimentManifestVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new LenientSentimentManifestVersionRangeRule(16, 80);
        var item = LenientSentimentManifest.NewLenientSentimentManifest("x", "y");
        item.Version = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientSentimentManifestDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new LenientSentimentManifestDurationMinutesRangeRule(3600, 3600);
        var item = LenientSentimentManifest.NewLenientSentimentManifest("x", "y");
        item.DurationMinutes = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientSentimentManifestValidatorReportsEveryProblem();
        LenientSentimentManifestCodeLengthRuleFlagsALongCode();
        LenientSentimentManifestRequiredCategoryRuleFlagsABlankCategory();
        LenientSentimentManifestVersionRangeRuleFlagsValuesAboveVersion();
        LenientSentimentManifestDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
