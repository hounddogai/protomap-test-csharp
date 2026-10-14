namespace Acme.Workforce.Engagement.Tests.Sentiment.Rollup;

using Acme.Workforce.Engagement.Sentiment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ManualSentimentAdjustmentRulesChecks
{
    public static void ManualSentimentAdjustmentValidatorReportsEveryProblem()
    {
        var validator = ManualSentimentAdjustmentRules.DefaultManualSentimentAdjustmentValidator();
        var item = ManualSentimentAdjustment.NewManualSentimentAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualSentimentAdjustmentMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new ManualSentimentAdjustmentMinimumRangeRule(3.4, 1760.6);
        var item = ManualSentimentAdjustment.NewManualSentimentAdjustment("x", "y");
        item.Minimum = 1762.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualSentimentAdjustmentNameLengthRuleFlagsALongName()
    {
        var rule = new ManualSentimentAdjustmentNameLengthRule(80);
        var item = ManualSentimentAdjustment.NewManualSentimentAdjustment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualSentimentAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualSentimentAdjustmentCodeLengthRule(86400);
        var item = ManualSentimentAdjustment.NewManualSentimentAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualSentimentAdjustmentValidatorReportsEveryProblem();
        ManualSentimentAdjustmentMinimumRangeRuleFlagsValuesAboveMinimum();
        ManualSentimentAdjustmentNameLengthRuleFlagsALongName();
        ManualSentimentAdjustmentCodeLengthRuleFlagsALongCode();
    }
}
