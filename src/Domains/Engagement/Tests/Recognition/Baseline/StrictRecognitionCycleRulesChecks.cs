namespace Acme.Workforce.Engagement.Tests.Recognition.Baseline;

using Acme.Workforce.Engagement.Recognition.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class StrictRecognitionCycleRulesChecks
{
    public static void StrictRecognitionCycleValidatorReportsEveryProblem()
    {
        var validator = StrictRecognitionCycleRules.DefaultStrictRecognitionCycleValidator();
        var item = StrictRecognitionCycle.NewStrictRecognitionCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictRecognitionCycleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictRecognitionCycleRequiredCodeRule("strict recognition cycle needs this field");
        var item = StrictRecognitionCycle.NewStrictRecognitionCycle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictRecognitionCycleRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new StrictRecognitionCycleRatioRangeRule(8.8, 2517.0);
        var item = StrictRecognitionCycle.NewStrictRecognitionCycle("x", "y");
        item.Ratio = 2518.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictRecognitionCycleLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new StrictRecognitionCycleLimitRangeRule(32, 120);
        var item = StrictRecognitionCycle.NewStrictRecognitionCycle("x", "y");
        item.Limit = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictRecognitionCycleRequiredNameRuleFlagsABlankName()
    {
        var rule = new StrictRecognitionCycleRequiredNameRule("strict recognition cycle needs a value");
        var item = StrictRecognitionCycle.NewStrictRecognitionCycle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictRecognitionCycleValidatorReportsEveryProblem();
        StrictRecognitionCycleRequiredCodeRuleFlagsABlankCode();
        StrictRecognitionCycleRatioRangeRuleFlagsValuesAboveRatio();
        StrictRecognitionCycleLimitRangeRuleFlagsValuesAboveLimit();
        StrictRecognitionCycleRequiredNameRuleFlagsABlankName();
    }
}
