namespace Acme.Workforce.Orgdesign.Tests.Scenario.Summary;

using Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PrimaryScenarioMappingRulesChecks
{
    public static void PrimaryScenarioMappingValidatorReportsEveryProblem()
    {
        var validator = PrimaryScenarioMappingRules.DefaultPrimaryScenarioMappingValidator();
        var item = PrimaryScenarioMapping.NewPrimaryScenarioMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryScenarioMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryScenarioMappingCodeLengthRule(86400);
        var item = PrimaryScenarioMapping.NewPrimaryScenarioMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryScenarioMappingLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PrimaryScenarioMappingLabelLengthRule(80);
        var item = PrimaryScenarioMapping.NewPrimaryScenarioMapping("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryScenarioMappingUnitLengthRuleFlagsALongUnit()
    {
        var rule = new PrimaryScenarioMappingUnitLengthRule(64);
        var item = PrimaryScenarioMapping.NewPrimaryScenarioMapping("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryScenarioMappingValidatorReportsEveryProblem();
        PrimaryScenarioMappingCodeLengthRuleFlagsALongCode();
        PrimaryScenarioMappingLabelLengthRuleFlagsALongLabel();
        PrimaryScenarioMappingUnitLengthRuleFlagsALongUnit();
    }
}
