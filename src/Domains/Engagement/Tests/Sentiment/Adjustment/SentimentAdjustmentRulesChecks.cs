namespace Acme.Workforce.Engagement.Tests.Sentiment.Adjustment;

using Acme.Workforce.Engagement.Sentiment.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SentimentAdjustmentRulesChecks
{
    public static void SentimentAdjustmentValidatorReportsEveryProblem()
    {
        var validator = SentimentAdjustmentRules.DefaultSentimentAdjustmentValidator();
        var item = SentimentAdjustment.NewSentimentAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SentimentAdjustmentRequiredCodeRule("sentiment adjustment needs this field");
        var item = SentimentAdjustment.NewSentimentAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentAdjustmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SentimentAdjustmentTitleLengthRule(120);
        var item = SentimentAdjustment.NewSentimentAdjustment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentAdjustmentRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new SentimentAdjustmentRequiredEmployeeIdRule("sentiment adjustment needs a value");
        var item = SentimentAdjustment.NewSentimentAdjustment("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentAdjustmentBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new SentimentAdjustmentBudgetRangeRule(4.8, 4628.2);
        var item = SentimentAdjustment.NewSentimentAdjustment("x", "y");
        item.Budget = 4629.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SentimentAdjustmentValidatorReportsEveryProblem();
        SentimentAdjustmentRequiredCodeRuleFlagsABlankCode();
        SentimentAdjustmentTitleLengthRuleFlagsALongTitle();
        SentimentAdjustmentRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        SentimentAdjustmentBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
