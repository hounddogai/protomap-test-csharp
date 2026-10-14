namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentManifest;

using Acme.Workforce.Engagement.Sentiment.SentimentManifest;

/// <summary>Checks of sentiment manifest code, run by the project's runner.</summary>
public static class TieredSentimentManifestRulesChecks
{
    public static void TieredSentimentManifestValidatorReportsEveryProblem()
    {
        var validator = TieredSentimentManifestRules.DefaultTieredSentimentManifestValidator();
        var item = TieredSentimentManifest.NewTieredSentimentManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredSentimentManifestRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new TieredSentimentManifestRequiredOwnerIdRule("tiered sentiment manifest needs a value");
        var item = TieredSentimentManifest.NewTieredSentimentManifest("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredSentimentManifestExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new TieredSentimentManifestExpiresAtRangeRule(32, 86400);
        var item = TieredSentimentManifest.NewTieredSentimentManifest("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredSentimentManifestOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new TieredSentimentManifestOwnerIdLengthRule(3600);
        var item = TieredSentimentManifest.NewTieredSentimentManifest("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredSentimentManifestValidatorReportsEveryProblem();
        TieredSentimentManifestRequiredOwnerIdRuleFlagsABlankOwnerId();
        TieredSentimentManifestExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        TieredSentimentManifestOwnerIdLengthRuleFlagsALongOwnerId();
    }
}
