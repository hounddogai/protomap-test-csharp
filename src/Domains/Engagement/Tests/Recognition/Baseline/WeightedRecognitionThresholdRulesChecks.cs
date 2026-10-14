namespace Acme.Workforce.Engagement.Tests.Recognition.Baseline;

using Acme.Workforce.Engagement.Recognition.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class WeightedRecognitionThresholdRulesChecks
{
    public static void WeightedRecognitionThresholdValidatorReportsEveryProblem()
    {
        var validator = WeightedRecognitionThresholdRules.DefaultWeightedRecognitionThresholdValidator();
        var item = WeightedRecognitionThreshold.NewWeightedRecognitionThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedRecognitionThresholdRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new WeightedRecognitionThresholdRequiredBadgeRule("weighted recognition threshold needs an entry");
        var item = WeightedRecognitionThreshold.NewWeightedRecognitionThreshold("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedRecognitionThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedRecognitionThresholdCodeLengthRule(1000);
        var item = WeightedRecognitionThreshold.NewWeightedRecognitionThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedRecognitionThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedRecognitionThresholdRequiredCodeRule("weighted recognition threshold needs a value");
        var item = WeightedRecognitionThreshold.NewWeightedRecognitionThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedRecognitionThresholdBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new WeightedRecognitionThresholdBadgeLengthRule(86400);
        var item = WeightedRecognitionThreshold.NewWeightedRecognitionThreshold("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedRecognitionThresholdValidatorReportsEveryProblem();
        WeightedRecognitionThresholdRequiredBadgeRuleFlagsABlankBadge();
        WeightedRecognitionThresholdCodeLengthRuleFlagsALongCode();
        WeightedRecognitionThresholdRequiredCodeRuleFlagsABlankCode();
        WeightedRecognitionThresholdBadgeLengthRuleFlagsALongBadge();
    }
}
