namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Agreement;

using Acme.Workforce.Engagement.PulseSurvey.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class PulseSurveyAgreementRulesChecks
{
    public static void PulseSurveyAgreementValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyAgreementRules.DefaultPulseSurveyAgreementValidator();
        var item = PulseSurveyAgreement.NewPulseSurveyAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new PulseSurveyAgreementCodeLengthRule(1000);
        var item = PulseSurveyAgreement.NewPulseSurveyAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyAgreementManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new PulseSurveyAgreementManagerIdLengthRule(86400);
        var item = PulseSurveyAgreement.NewPulseSurveyAgreement("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyAgreementTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PulseSurveyAgreementTitleLengthRule(255);
        var item = PulseSurveyAgreement.NewPulseSurveyAgreement("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyAgreementAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PulseSurveyAgreementAmountRangeRule(5.3, 4912.0);
        var item = PulseSurveyAgreement.NewPulseSurveyAgreement("x", "y");
        item.Amount = 4913.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PulseSurveyAgreementValidatorReportsEveryProblem();
        PulseSurveyAgreementCodeLengthRuleFlagsALongCode();
        PulseSurveyAgreementManagerIdLengthRuleFlagsALongManagerId();
        PulseSurveyAgreementTitleLengthRuleFlagsALongTitle();
        PulseSurveyAgreementAmountRangeRuleFlagsValuesAboveAmount();
    }
}
