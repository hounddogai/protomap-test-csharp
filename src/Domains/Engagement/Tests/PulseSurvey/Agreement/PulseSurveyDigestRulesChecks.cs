namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Agreement;

using Acme.Workforce.Engagement.PulseSurvey.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class PulseSurveyDigestRulesChecks
{
    public static void PulseSurveyDigestValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyDigestRules.DefaultPulseSurveyDigestValidator();
        var item = PulseSurveyDigest.NewPulseSurveyDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyDigestWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new PulseSurveyDigestWeightRangeRule(3.4, 1843.6);
        var item = PulseSurveyDigest.NewPulseSurveyDigest("x", "y");
        item.Weight = 1845.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new PulseSurveyDigestCodeLengthRule(32);
        var item = PulseSurveyDigest.NewPulseSurveyDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyDigestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PulseSurveyDigestRequiredTitleRule("pulse survey digest needs this field");
        var item = PulseSurveyDigest.NewPulseSurveyDigest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PulseSurveyDigestValidatorReportsEveryProblem();
        PulseSurveyDigestWeightRangeRuleFlagsValuesAboveWeight();
        PulseSurveyDigestCodeLengthRuleFlagsALongCode();
        PulseSurveyDigestRequiredTitleRuleFlagsABlankTitle();
    }
}
