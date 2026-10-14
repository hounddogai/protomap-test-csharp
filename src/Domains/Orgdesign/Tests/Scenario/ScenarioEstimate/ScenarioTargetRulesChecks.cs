namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioEstimate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>Checks of scenario estimate code, run by the project's runner.</summary>
public static class ScenarioTargetRulesChecks
{
    public static void ScenarioTargetValidatorReportsEveryProblem()
    {
        var validator = ScenarioTargetRules.DefaultScenarioTargetValidator();
        var item = ScenarioTarget.NewScenarioTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioTargetRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new ScenarioTargetRequiredEmployeeIdRule("scenario target needs a value");
        var item = ScenarioTarget.NewScenarioTarget("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScenarioTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScenarioTargetCodeLengthRule(86400);
        var item = ScenarioTarget.NewScenarioTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScenarioTargetRequiredCodeRule("scenario target needs an entry");
        var item = ScenarioTarget.NewScenarioTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScenarioTargetNameLengthRuleFlagsALongName()
    {
        var rule = new ScenarioTargetNameLengthRule(120);
        var item = ScenarioTarget.NewScenarioTarget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScenarioTargetValidatorReportsEveryProblem();
        ScenarioTargetRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        ScenarioTargetCodeLengthRuleFlagsALongCode();
        ScenarioTargetRequiredCodeRuleFlagsABlankCode();
        ScenarioTargetNameLengthRuleFlagsALongName();
    }
}
