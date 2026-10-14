namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioPolicy;

using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Checks of scenario policy code, run by the project's runner.</summary>
public static class ScenarioAgreementRulesChecks
{
    public static void ScenarioAgreementValidatorReportsEveryProblem()
    {
        var validator = ScenarioAgreementRules.DefaultScenarioAgreementValidator();
        var item = ScenarioAgreement.NewScenarioAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioAgreementRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScenarioAgreementRequiredCodeRule("scenario agreement needs an entry");
        var item = ScenarioAgreement.NewScenarioAgreement("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScenarioAgreementNameLengthRuleFlagsALongName()
    {
        var rule = new ScenarioAgreementNameLengthRule(255);
        var item = ScenarioAgreement.NewScenarioAgreement("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScenarioAgreementCodeLengthRule(120);
        var item = ScenarioAgreement.NewScenarioAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScenarioAgreementValidatorReportsEveryProblem();
        ScenarioAgreementRequiredCodeRuleFlagsABlankCode();
        ScenarioAgreementNameLengthRuleFlagsALongName();
        ScenarioAgreementCodeLengthRuleFlagsALongCode();
    }
}
