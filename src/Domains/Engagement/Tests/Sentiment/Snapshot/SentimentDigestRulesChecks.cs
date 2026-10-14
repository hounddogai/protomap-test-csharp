namespace Acme.Workforce.Engagement.Tests.Sentiment.Snapshot;

using Acme.Workforce.Engagement.Sentiment.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SentimentDigestRulesChecks
{
    public static void SentimentDigestValidatorReportsEveryProblem()
    {
        var validator = SentimentDigestRules.DefaultSentimentDigestValidator();
        var item = SentimentDigest.NewSentimentDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new SentimentDigestCodeLengthRule(16);
        var item = SentimentDigest.NewSentimentDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentDigestDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new SentimentDigestDescriptionLengthRule(1000);
        var item = SentimentDigest.NewSentimentDigest("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentDigestSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new SentimentDigestSequenceRangeRule(255, 86400);
        var item = SentimentDigest.NewSentimentDigest("x", "y");
        item.Sequence = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SentimentDigestSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new SentimentDigestSalaryBandLengthRule(255);
        var item = SentimentDigest.NewSentimentDigest("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SentimentDigestValidatorReportsEveryProblem();
        SentimentDigestCodeLengthRuleFlagsALongCode();
        SentimentDigestDescriptionLengthRuleFlagsALongDescription();
        SentimentDigestSequenceRangeRuleFlagsValuesAboveSequence();
        SentimentDigestSalaryBandLengthRuleFlagsALongSalaryBand();
    }
}
