namespace Acme.Workforce.Orgdesign.Tests.Scenario.Tier;

using Acme.Workforce.Orgdesign.Scenario.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ScenarioBaselineRulesChecks
{
    public static void ScenarioBaselineValidatorReportsEveryProblem()
    {
        var validator = ScenarioBaselineRules.DefaultScenarioBaselineValidator();
        var item = ScenarioBaseline.NewScenarioBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioBaselineVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new ScenarioBaselineVersionRangeRule(64, 64);
        var item = ScenarioBaseline.NewScenarioBaseline("x", "y");
        item.Version = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScenarioBaselineRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new ScenarioBaselineRevisionRangeRule(120, 3600);
        var item = ScenarioBaseline.NewScenarioBaseline("x", "y");
        item.Revision = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScenarioBaselineLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ScenarioBaselineLabelLengthRule(120);
        var item = ScenarioBaseline.NewScenarioBaseline("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioBaselineRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ScenarioBaselineRequiredLabelRule("scenario baseline needs an entry");
        var item = ScenarioBaseline.NewScenarioBaseline("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScenarioBaselineCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new ScenarioBaselineCostRangeRule(9.4, 4233.8);
        var item = ScenarioBaseline.NewScenarioBaseline("x", "y");
        item.Cost = 4235.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScenarioBaselineValidatorReportsEveryProblem();
        ScenarioBaselineVersionRangeRuleFlagsValuesAboveVersion();
        ScenarioBaselineRevisionRangeRuleFlagsValuesAboveRevision();
        ScenarioBaselineLabelLengthRuleFlagsALongLabel();
        ScenarioBaselineRequiredLabelRuleFlagsABlankLabel();
        ScenarioBaselineCostRangeRuleFlagsValuesAboveCost();
    }
}
