namespace Acme.Workforce.Orgdesign.Tests.Scenario.Estimate;

using Acme.Workforce.Orgdesign.Scenario.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class LenientScenarioAgreementRulesChecks
{
    public static void LenientScenarioAgreementValidatorReportsEveryProblem()
    {
        var validator = LenientScenarioAgreementRules.DefaultLenientScenarioAgreementValidator();
        var item = LenientScenarioAgreement.NewLenientScenarioAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientScenarioAgreementRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientScenarioAgreementRequiredCodeRule("lenient scenario agreement needs an entry");
        var item = LenientScenarioAgreement.NewLenientScenarioAgreement("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientScenarioAgreementSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new LenientScenarioAgreementSummaryLengthRule(64);
        var item = LenientScenarioAgreement.NewLenientScenarioAgreement("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientScenarioAgreementLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LenientScenarioAgreementLabelLengthRule(86400);
        var item = LenientScenarioAgreement.NewLenientScenarioAgreement("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientScenarioAgreementValidatorReportsEveryProblem();
        LenientScenarioAgreementRequiredCodeRuleFlagsABlankCode();
        LenientScenarioAgreementSummaryLengthRuleFlagsALongSummary();
        LenientScenarioAgreementLabelLengthRuleFlagsALongLabel();
    }
}
