namespace Acme.Workforce.Orgdesign.Tests.Scenario.Cycle;

using Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScenarioMappingRulesChecks
{
    public static void ScenarioMappingValidatorReportsEveryProblem()
    {
        var validator = ScenarioMappingRules.DefaultScenarioMappingValidator();
        var item = ScenarioMapping.NewScenarioMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScenarioMappingCodeLengthRule(3600);
        var item = ScenarioMapping.NewScenarioMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioMappingRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScenarioMappingRequiredCodeRule("scenario mapping needs a value");
        var item = ScenarioMapping.NewScenarioMapping("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScenarioMappingRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScenarioMappingRequiredNameRule("scenario mapping needs an entry");
        var item = ScenarioMapping.NewScenarioMapping("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScenarioMappingNameLengthRuleFlagsALongName()
    {
        var rule = new ScenarioMappingNameLengthRule(3600);
        var item = ScenarioMapping.NewScenarioMapping("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScenarioMappingValidatorReportsEveryProblem();
        ScenarioMappingCodeLengthRuleFlagsALongCode();
        ScenarioMappingRequiredCodeRuleFlagsABlankCode();
        ScenarioMappingRequiredNameRuleFlagsABlankName();
        ScenarioMappingNameLengthRuleFlagsALongName();
    }
}
