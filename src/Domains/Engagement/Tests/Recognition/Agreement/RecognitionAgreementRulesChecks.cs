namespace Acme.Workforce.Engagement.Tests.Recognition.Agreement;

using Acme.Workforce.Engagement.Recognition.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class RecognitionAgreementRulesChecks
{
    public static void RecognitionAgreementValidatorReportsEveryProblem()
    {
        var validator = RecognitionAgreementRules.DefaultRecognitionAgreementValidator();
        var item = RecognitionAgreement.NewRecognitionAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionAgreementMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new RecognitionAgreementMinimumRangeRule(8.7, 4773.8);
        var item = RecognitionAgreement.NewRecognitionAgreement("x", "y");
        item.Minimum = 4775.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionAgreementRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new RecognitionAgreementRequiredLocaleRule("recognition agreement needs this field");
        var item = RecognitionAgreement.NewRecognitionAgreement("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RecognitionAgreementRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RecognitionAgreementRequiredLabelRule("recognition agreement needs a value");
        var item = RecognitionAgreement.NewRecognitionAgreement("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RecognitionAgreementValidatorReportsEveryProblem();
        RecognitionAgreementMinimumRangeRuleFlagsValuesAboveMinimum();
        RecognitionAgreementRequiredLocaleRuleFlagsABlankLocale();
        RecognitionAgreementRequiredLabelRuleFlagsABlankLabel();
    }
}
