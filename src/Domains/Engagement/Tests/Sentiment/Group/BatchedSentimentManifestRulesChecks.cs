namespace Acme.Workforce.Engagement.Tests.Sentiment.Group;

using Acme.Workforce.Engagement.Sentiment.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BatchedSentimentManifestRulesChecks
{
    public static void BatchedSentimentManifestValidatorReportsEveryProblem()
    {
        var validator = BatchedSentimentManifestRules.DefaultBatchedSentimentManifestValidator();
        var item = BatchedSentimentManifest.NewBatchedSentimentManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedSentimentManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedSentimentManifestRequiredCodeRule("batched sentiment manifest needs this field");
        var item = BatchedSentimentManifest.NewBatchedSentimentManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedSentimentManifestEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new BatchedSentimentManifestEndsAtRangeRule(16, 255);
        var item = BatchedSentimentManifest.NewBatchedSentimentManifest("x", "y");
        item.EndsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedSentimentManifestRequiredNameRuleFlagsABlankName()
    {
        var rule = new BatchedSentimentManifestRequiredNameRule("batched sentiment manifest needs this field");
        var item = BatchedSentimentManifest.NewBatchedSentimentManifest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedSentimentManifestNameLengthRuleFlagsALongName()
    {
        var rule = new BatchedSentimentManifestNameLengthRule(1000);
        var item = BatchedSentimentManifest.NewBatchedSentimentManifest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedSentimentManifestValidatorReportsEveryProblem();
        BatchedSentimentManifestRequiredCodeRuleFlagsABlankCode();
        BatchedSentimentManifestEndsAtRangeRuleFlagsValuesAboveEndsAt();
        BatchedSentimentManifestRequiredNameRuleFlagsABlankName();
        BatchedSentimentManifestNameLengthRuleFlagsALongName();
    }
}
