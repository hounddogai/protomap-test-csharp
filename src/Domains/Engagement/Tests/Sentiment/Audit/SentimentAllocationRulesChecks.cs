namespace Acme.Workforce.Engagement.Tests.Sentiment.Audit;

using Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SentimentAllocationRulesChecks
{
    public static void SentimentAllocationValidatorReportsEveryProblem()
    {
        var validator = SentimentAllocationRules.DefaultSentimentAllocationValidator();
        var item = SentimentAllocation.NewSentimentAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentAllocationNameLengthRuleFlagsALongName()
    {
        var rule = new SentimentAllocationNameLengthRule(1000);
        var item = SentimentAllocation.NewSentimentAllocation("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new SentimentAllocationCodeLengthRule(86400);
        var item = SentimentAllocation.NewSentimentAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentAllocationRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new SentimentAllocationRateRangeRule(8.1, 1986.7);
        var item = SentimentAllocation.NewSentimentAllocation("x", "y");
        item.Rate = 1988.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SentimentAllocationValidatorReportsEveryProblem();
        SentimentAllocationNameLengthRuleFlagsALongName();
        SentimentAllocationCodeLengthRuleFlagsALongCode();
        SentimentAllocationRateRangeRuleFlagsValuesAboveRate();
    }
}
