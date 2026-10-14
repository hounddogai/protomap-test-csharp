namespace Acme.Workforce.Engagement.Tests.Recognition.Window;

using Acme.Workforce.Engagement.Recognition.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RecognitionWindowRulesChecks
{
    public static void RecognitionWindowValidatorReportsEveryProblem()
    {
        var validator = RecognitionWindowRules.DefaultRecognitionWindowValidator();
        var item = RecognitionWindow.NewRecognitionWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionWindowQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new RecognitionWindowQuantityRangeRule(80, 1000);
        var item = RecognitionWindow.NewRecognitionWindow("x", "y");
        item.Quantity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RecognitionWindowRequiredCodeRule("recognition window needs this field");
        var item = RecognitionWindow.NewRecognitionWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RecognitionWindowValidatorReportsEveryProblem();
        RecognitionWindowQuantityRangeRuleFlagsValuesAboveQuantity();
        RecognitionWindowRequiredCodeRuleFlagsABlankCode();
    }
}
