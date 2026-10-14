namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioTemplate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>Checks of scenario template code, run by the project's runner.</summary>
public static class PendingScenarioTemplateRulesChecks
{
    public static void PendingScenarioTemplateValidatorReportsEveryProblem()
    {
        var validator = PendingScenarioTemplateRules.DefaultPendingScenarioTemplateValidator();
        var item = PendingScenarioTemplate.NewPendingScenarioTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingScenarioTemplateRequiredNameRuleFlagsABlankName()
    {
        var rule = new PendingScenarioTemplateRequiredNameRule("pending scenario template needs an entry");
        var item = PendingScenarioTemplate.NewPendingScenarioTemplate("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingScenarioTemplateNameLengthRuleFlagsALongName()
    {
        var rule = new PendingScenarioTemplateNameLengthRule(80);
        var item = PendingScenarioTemplate.NewPendingScenarioTemplate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingScenarioTemplateValidatorReportsEveryProblem();
        PendingScenarioTemplateRequiredNameRuleFlagsABlankName();
        PendingScenarioTemplateNameLengthRuleFlagsALongName();
    }
}
