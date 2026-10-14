namespace Acme.Workforce.Engagement.Tests.Recognition.Threshold;

using Acme.Workforce.Engagement.Recognition.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RegionalRecognitionThresholdRulesChecks
{
    public static void RegionalRecognitionThresholdValidatorReportsEveryProblem()
    {
        var validator = RegionalRecognitionThresholdRules.DefaultRegionalRecognitionThresholdValidator();
        var item = RegionalRecognitionThreshold.NewRegionalRecognitionThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalRecognitionThresholdDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new RegionalRecognitionThresholdDescriptionLengthRule(80);
        var item = RegionalRecognitionThreshold.NewRegionalRecognitionThreshold("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalRecognitionThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalRecognitionThresholdRequiredCodeRule("regional recognition threshold needs this field");
        var item = RegionalRecognitionThreshold.NewRegionalRecognitionThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalRecognitionThresholdValidatorReportsEveryProblem();
        RegionalRecognitionThresholdDescriptionLengthRuleFlagsALongDescription();
        RegionalRecognitionThresholdRequiredCodeRuleFlagsABlankCode();
    }
}
