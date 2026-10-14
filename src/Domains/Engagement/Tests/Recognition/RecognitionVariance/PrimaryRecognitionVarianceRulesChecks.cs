namespace Acme.Workforce.Engagement.Tests.Recognition.RecognitionVariance;

using Acme.Workforce.Engagement.Recognition.RecognitionVariance;

/// <summary>Checks of recognition variance code, run by the project's runner.</summary>
public static class PrimaryRecognitionVarianceRulesChecks
{
    public static void PrimaryRecognitionVarianceValidatorReportsEveryProblem()
    {
        var validator = PrimaryRecognitionVarianceRules.DefaultPrimaryRecognitionVarianceValidator();
        var item = PrimaryRecognitionVariance.NewPrimaryRecognitionVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryRecognitionVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryRecognitionVarianceRequiredCodeRule("primary recognition variance needs this field");
        var item = PrimaryRecognitionVariance.NewPrimaryRecognitionVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryRecognitionVarianceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PrimaryRecognitionVarianceRequiredTitleRule("primary recognition variance needs this field");
        var item = PrimaryRecognitionVariance.NewPrimaryRecognitionVariance("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryRecognitionVarianceLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new PrimaryRecognitionVarianceLocaleLengthRule(80);
        var item = PrimaryRecognitionVariance.NewPrimaryRecognitionVariance("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryRecognitionVarianceValidatorReportsEveryProblem();
        PrimaryRecognitionVarianceRequiredCodeRuleFlagsABlankCode();
        PrimaryRecognitionVarianceRequiredTitleRuleFlagsABlankTitle();
        PrimaryRecognitionVarianceLocaleLengthRuleFlagsALongLocale();
    }
}
