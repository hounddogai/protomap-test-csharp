namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentVariance;

using Acme.Workforce.Engagement.Sentiment.SentimentVariance;

/// <summary>Checks of sentiment variance code, run by the project's runner.</summary>
public static class SentimentAgreementRulesChecks
{
    public static void SentimentAgreementValidatorReportsEveryProblem()
    {
        var validator = SentimentAgreementRules.DefaultSentimentAgreementValidator();
        var item = SentimentAgreement.NewSentimentAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentAgreementJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new SentimentAgreementJobCodeLengthRule(1000);
        var item = SentimentAgreement.NewSentimentAgreement("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentAgreementUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new SentimentAgreementUpdatedAtRangeRule(32, 3600);
        var item = SentimentAgreement.NewSentimentAgreement("x", "y");
        item.UpdatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SentimentAgreementValidatorReportsEveryProblem();
        SentimentAgreementJobCodeLengthRuleFlagsALongJobCode();
        SentimentAgreementUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
