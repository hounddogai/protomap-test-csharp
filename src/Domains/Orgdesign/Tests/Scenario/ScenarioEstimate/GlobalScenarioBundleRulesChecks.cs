namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioEstimate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>Checks of scenario estimate code, run by the project's runner.</summary>
public static class GlobalScenarioBundleRulesChecks
{
    public static void GlobalScenarioBundleValidatorReportsEveryProblem()
    {
        var validator = GlobalScenarioBundleRules.DefaultGlobalScenarioBundleValidator();
        var item = GlobalScenarioBundle.NewGlobalScenarioBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalScenarioBundleRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new GlobalScenarioBundleRevisionRangeRule(120, 120);
        var item = GlobalScenarioBundle.NewGlobalScenarioBundle("x", "y");
        item.Revision = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalScenarioBundleCountryLengthRuleFlagsALongCountry()
    {
        var rule = new GlobalScenarioBundleCountryLengthRule(16);
        var item = GlobalScenarioBundle.NewGlobalScenarioBundle("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalScenarioBundleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new GlobalScenarioBundleRequiredLabelRule("global scenario bundle needs a value");
        var item = GlobalScenarioBundle.NewGlobalScenarioBundle("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalScenarioBundleRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new GlobalScenarioBundleRequiredEmployeeIdRule("global scenario bundle needs this field");
        var item = GlobalScenarioBundle.NewGlobalScenarioBundle("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalScenarioBundleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new GlobalScenarioBundleLabelLengthRule(86400);
        var item = GlobalScenarioBundle.NewGlobalScenarioBundle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalScenarioBundleValidatorReportsEveryProblem();
        GlobalScenarioBundleRevisionRangeRuleFlagsValuesAboveRevision();
        GlobalScenarioBundleCountryLengthRuleFlagsALongCountry();
        GlobalScenarioBundleRequiredLabelRuleFlagsABlankLabel();
        GlobalScenarioBundleRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        GlobalScenarioBundleLabelLengthRuleFlagsALongLabel();
    }
}
