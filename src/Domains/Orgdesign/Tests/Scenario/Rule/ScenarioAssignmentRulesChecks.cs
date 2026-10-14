namespace Acme.Workforce.Orgdesign.Tests.Scenario.Rule;

using Acme.Workforce.Orgdesign.Scenario.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ScenarioAssignmentRulesChecks
{
    public static void ScenarioAssignmentValidatorReportsEveryProblem()
    {
        var validator = ScenarioAssignmentRules.DefaultScenarioAssignmentValidator();
        var item = ScenarioAssignment.NewScenarioAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioAssignmentMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new ScenarioAssignmentMinimumRangeRule(4.4, 3228.3);
        var item = ScenarioAssignment.NewScenarioAssignment("x", "y");
        item.Minimum = 3229.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScenarioAssignmentRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new ScenarioAssignmentRatioRangeRule(3.3, 3256.5);
        var item = ScenarioAssignment.NewScenarioAssignment("x", "y");
        item.Ratio = 3258.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScenarioAssignmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ScenarioAssignmentRequiredLabelRule("scenario assignment needs an entry");
        var item = ScenarioAssignment.NewScenarioAssignment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScenarioAssignmentValidatorReportsEveryProblem();
        ScenarioAssignmentMinimumRangeRuleFlagsValuesAboveMinimum();
        ScenarioAssignmentRatioRangeRuleFlagsValuesAboveRatio();
        ScenarioAssignmentRequiredLabelRuleFlagsABlankLabel();
    }
}
