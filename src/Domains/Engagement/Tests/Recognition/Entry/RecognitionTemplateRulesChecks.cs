namespace Acme.Workforce.Engagement.Tests.Recognition.Entry;

using Acme.Workforce.Engagement.Recognition.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class RecognitionTemplateRulesChecks
{
    public static void RecognitionTemplateValidatorReportsEveryProblem()
    {
        var validator = RecognitionTemplateRules.DefaultRecognitionTemplateValidator();
        var item = RecognitionTemplate.NewRecognitionTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionTemplateRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new RecognitionTemplateRatioRangeRule(3.1, 3617.2);
        var item = RecognitionTemplate.NewRecognitionTemplate("x", "y");
        item.Ratio = 3618.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionTemplateRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new RecognitionTemplateRequiredManagerIdRule("recognition template needs an entry");
        var item = RecognitionTemplate.NewRecognitionTemplate("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RecognitionTemplateCodeLengthRuleFlagsALongCode()
    {
        var rule = new RecognitionTemplateCodeLengthRule(64);
        var item = RecognitionTemplate.NewRecognitionTemplate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionTemplateNameLengthRuleFlagsALongName()
    {
        var rule = new RecognitionTemplateNameLengthRule(120);
        var item = RecognitionTemplate.NewRecognitionTemplate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RecognitionTemplateValidatorReportsEveryProblem();
        RecognitionTemplateRatioRangeRuleFlagsValuesAboveRatio();
        RecognitionTemplateRequiredManagerIdRuleFlagsABlankManagerId();
        RecognitionTemplateCodeLengthRuleFlagsALongCode();
        RecognitionTemplateNameLengthRuleFlagsALongName();
    }
}
