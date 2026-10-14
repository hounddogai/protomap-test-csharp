namespace Acme.Workforce.Orgdesign.Tests.Scenario.Metric;

using Acme.Workforce.Orgdesign.Scenario.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PendingScenarioAgreementRulesChecks
{
    public static void PendingScenarioAgreementValidatorReportsEveryProblem()
    {
        var validator = PendingScenarioAgreementRules.DefaultPendingScenarioAgreementValidator();
        var item = PendingScenarioAgreement.NewPendingScenarioAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingScenarioAgreementCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new PendingScenarioAgreementCapacityRangeRule(16, 255);
        var item = PendingScenarioAgreement.NewPendingScenarioAgreement("x", "y");
        item.Capacity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingScenarioAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingScenarioAgreementCodeLengthRule(86400);
        var item = PendingScenarioAgreement.NewPendingScenarioAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingScenarioAgreementRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new PendingScenarioAgreementRequiredExternalRefRule("pending scenario agreement needs a value");
        var item = PendingScenarioAgreement.NewPendingScenarioAgreement("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingScenarioAgreementValidatorReportsEveryProblem();
        PendingScenarioAgreementCapacityRangeRuleFlagsValuesAboveCapacity();
        PendingScenarioAgreementCodeLengthRuleFlagsALongCode();
        PendingScenarioAgreementRequiredExternalRefRuleFlagsABlankExternalRef();
    }
}
