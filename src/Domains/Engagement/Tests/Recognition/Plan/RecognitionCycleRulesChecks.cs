namespace Acme.Workforce.Engagement.Tests.Recognition.Plan;

using Acme.Workforce.Engagement.Recognition.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RecognitionCycleRulesChecks
{
    public static void RecognitionCycleValidatorReportsEveryProblem()
    {
        var validator = RecognitionCycleRules.DefaultRecognitionCycleValidator();
        var item = RecognitionCycle.NewRecognitionCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionCycleMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new RecognitionCycleMarginRangeRule(5.1, 1014.0);
        var item = RecognitionCycle.NewRecognitionCycle("x", "y");
        item.Margin = 1015.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionCycleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RecognitionCycleRequiredCodeRule("recognition cycle needs an entry");
        var item = RecognitionCycle.NewRecognitionCycle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RecognitionCycleGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new RecognitionCycleGraceDaysRangeRule(80, 86400);
        var item = RecognitionCycle.NewRecognitionCycle("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RecognitionCycleValidatorReportsEveryProblem();
        RecognitionCycleMarginRangeRuleFlagsValuesAboveMargin();
        RecognitionCycleRequiredCodeRuleFlagsABlankCode();
        RecognitionCycleGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
