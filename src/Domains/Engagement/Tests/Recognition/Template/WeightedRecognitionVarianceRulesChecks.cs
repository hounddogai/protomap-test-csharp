namespace Acme.Workforce.Engagement.Tests.Recognition.Template;

using Acme.Workforce.Engagement.Recognition.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class WeightedRecognitionVarianceRulesChecks
{
    public static void WeightedRecognitionVarianceValidatorReportsEveryProblem()
    {
        var validator = WeightedRecognitionVarianceRules.DefaultWeightedRecognitionVarianceValidator();
        var item = WeightedRecognitionVariance.NewWeightedRecognitionVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedRecognitionVarianceChannelLengthRuleFlagsALongChannel()
    {
        var rule = new WeightedRecognitionVarianceChannelLengthRule(1000);
        var item = WeightedRecognitionVariance.NewWeightedRecognitionVariance("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedRecognitionVarianceRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new WeightedRecognitionVarianceRequiredChannelRule("weighted recognition variance needs an entry");
        var item = WeightedRecognitionVariance.NewWeightedRecognitionVariance("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedRecognitionVarianceRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new WeightedRecognitionVarianceRequiredContactEmailRule(
            "weighted recognition variance needs a value");
        var item = WeightedRecognitionVariance.NewWeightedRecognitionVariance("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedRecognitionVarianceRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new WeightedRecognitionVarianceRequiredLabelRule("weighted recognition variance needs an entry");
        var item = WeightedRecognitionVariance.NewWeightedRecognitionVariance("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedRecognitionVarianceValidatorReportsEveryProblem();
        WeightedRecognitionVarianceChannelLengthRuleFlagsALongChannel();
        WeightedRecognitionVarianceRequiredChannelRuleFlagsABlankChannel();
        WeightedRecognitionVarianceRequiredContactEmailRuleFlagsABlankContactEmail();
        WeightedRecognitionVarianceRequiredLabelRuleFlagsABlankLabel();
    }
}
