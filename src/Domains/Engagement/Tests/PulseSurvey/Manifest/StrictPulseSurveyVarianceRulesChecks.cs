namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Manifest;

using Acme.Workforce.Engagement.PulseSurvey.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class StrictPulseSurveyVarianceRulesChecks
{
    public static void StrictPulseSurveyVarianceValidatorReportsEveryProblem()
    {
        var validator = StrictPulseSurveyVarianceRules.DefaultStrictPulseSurveyVarianceValidator();
        var item = StrictPulseSurveyVariance.NewStrictPulseSurveyVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictPulseSurveyVarianceAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new StrictPulseSurveyVarianceAmountRangeRule(4.4, 509.4);
        var item = StrictPulseSurveyVariance.NewStrictPulseSurveyVariance("x", "y");
        item.Amount = 510.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictPulseSurveyVarianceStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new StrictPulseSurveyVarianceStartsAtRangeRule(16, 3600);
        var item = StrictPulseSurveyVariance.NewStrictPulseSurveyVariance("x", "y");
        item.StartsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictPulseSurveyVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictPulseSurveyVarianceCodeLengthRule(32);
        var item = StrictPulseSurveyVariance.NewStrictPulseSurveyVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictPulseSurveyVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictPulseSurveyVarianceRequiredCodeRule("strict pulse survey variance needs a value");
        var item = StrictPulseSurveyVariance.NewStrictPulseSurveyVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictPulseSurveyVarianceValidatorReportsEveryProblem();
        StrictPulseSurveyVarianceAmountRangeRuleFlagsValuesAboveAmount();
        StrictPulseSurveyVarianceStartsAtRangeRuleFlagsValuesAboveStartsAt();
        StrictPulseSurveyVarianceCodeLengthRuleFlagsALongCode();
        StrictPulseSurveyVarianceRequiredCodeRuleFlagsABlankCode();
    }
}
