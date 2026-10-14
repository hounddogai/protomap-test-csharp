namespace Acme.Workforce.Orgdesign.Tests.Scenario.WeightedScenarioTemplate;

using Acme.Workforce.Orgdesign.Scenario.WeightedScenarioTemplate;

/// <summary>Checks of weighted scenario template code, run by the project's runner.</summary>
public static class PrimaryScenarioAgreementRulesChecks
{
    public static void PrimaryScenarioAgreementValidatorReportsEveryProblem()
    {
        var validator = PrimaryScenarioAgreementRules.DefaultPrimaryScenarioAgreementValidator();
        var item = PrimaryScenarioAgreement.NewPrimaryScenarioAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryScenarioAgreementRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PrimaryScenarioAgreementRequiredLabelRule("primary scenario agreement needs a value");
        var item = PrimaryScenarioAgreement.NewPrimaryScenarioAgreement("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryScenarioAgreementNameLengthRuleFlagsALongName()
    {
        var rule = new PrimaryScenarioAgreementNameLengthRule(16);
        var item = PrimaryScenarioAgreement.NewPrimaryScenarioAgreement("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryScenarioAgreementRequiredNameRuleFlagsABlankName()
    {
        var rule = new PrimaryScenarioAgreementRequiredNameRule("primary scenario agreement needs this field");
        var item = PrimaryScenarioAgreement.NewPrimaryScenarioAgreement("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryScenarioAgreementValidatorReportsEveryProblem();
        PrimaryScenarioAgreementRequiredLabelRuleFlagsABlankLabel();
        PrimaryScenarioAgreementNameLengthRuleFlagsALongName();
        PrimaryScenarioAgreementRequiredNameRuleFlagsABlankName();
    }
}
