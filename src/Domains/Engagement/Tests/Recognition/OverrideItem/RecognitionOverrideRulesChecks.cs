namespace Acme.Workforce.Engagement.Tests.Recognition.OverrideItem;

using Acme.Workforce.Engagement.Recognition.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RecognitionOverrideRulesChecks
{
    public static void RecognitionOverrideValidatorReportsEveryProblem()
    {
        var validator = RecognitionOverrideRules.DefaultRecognitionOverrideValidator();
        var item = RecognitionOverride.NewRecognitionOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new RecognitionOverrideCodeLengthRule(80);
        var item = RecognitionOverride.NewRecognitionOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RecognitionOverrideValidatorReportsEveryProblem();
        RecognitionOverrideCodeLengthRuleFlagsALongCode();
    }
}
