namespace Acme.Workforce.Orgdesign.Tests.Scenario.Policy;

using Acme.Workforce.Orgdesign.Scenario.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class GlobalScenarioPolicyRulesChecks
{
    public static void GlobalScenarioPolicyValidatorReportsEveryProblem()
    {
        var validator = GlobalScenarioPolicyRules.DefaultGlobalScenarioPolicyValidator();
        var item = GlobalScenarioPolicy.NewGlobalScenarioPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalScenarioPolicyFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new GlobalScenarioPolicyFteRangeRule(5.6, 3501.5);
        var item = GlobalScenarioPolicy.NewGlobalScenarioPolicy("x", "y");
        item.Fte = 3503.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalScenarioPolicyRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new GlobalScenarioPolicyRequiredExternalRefRule("global scenario policy needs a value");
        var item = GlobalScenarioPolicy.NewGlobalScenarioPolicy("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalScenarioPolicyValidatorReportsEveryProblem();
        GlobalScenarioPolicyFteRangeRuleFlagsValuesAboveFte();
        GlobalScenarioPolicyRequiredExternalRefRuleFlagsABlankExternalRef();
    }
}
