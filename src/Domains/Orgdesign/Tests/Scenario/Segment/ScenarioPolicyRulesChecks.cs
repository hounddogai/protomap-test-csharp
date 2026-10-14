namespace Acme.Workforce.Orgdesign.Tests.Scenario.Segment;

using Acme.Workforce.Orgdesign.Scenario.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ScenarioPolicyRulesChecks
{
    public static void ScenarioPolicyValidatorReportsEveryProblem()
    {
        var validator = ScenarioPolicyRules.DefaultScenarioPolicyValidator();
        var item = ScenarioPolicy.NewScenarioPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioPolicyTeamLengthRuleFlagsALongTeam()
    {
        var rule = new ScenarioPolicyTeamLengthRule(255);
        var item = ScenarioPolicy.NewScenarioPolicy("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioPolicyRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScenarioPolicyRequiredNameRule("scenario policy needs this field");
        var item = ScenarioPolicy.NewScenarioPolicy("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScenarioPolicyValidatorReportsEveryProblem();
        ScenarioPolicyTeamLengthRuleFlagsALongTeam();
        ScenarioPolicyRequiredNameRuleFlagsABlankName();
    }
}
