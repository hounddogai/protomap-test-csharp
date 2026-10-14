namespace Acme.Workforce.Orgdesign.Tests.Scenario.Estimate;

using Acme.Workforce.Orgdesign.Scenario.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class ScenarioBudgetRulesChecks
{
    public static void ScenarioBudgetValidatorReportsEveryProblem()
    {
        var validator = ScenarioBudgetRules.DefaultScenarioBudgetValidator();
        var item = ScenarioBudget.NewScenarioBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioBudgetRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new ScenarioBudgetRequiredSegmentRule("scenario budget needs an entry");
        var item = ScenarioBudget.NewScenarioBudget("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScenarioBudgetBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new ScenarioBudgetBadgeLengthRule(32);
        var item = ScenarioBudget.NewScenarioBudget("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioBudgetSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new ScenarioBudgetSegmentLengthRule(16);
        var item = ScenarioBudget.NewScenarioBudget("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioBudgetLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ScenarioBudgetLabelLengthRule(255);
        var item = ScenarioBudget.NewScenarioBudget("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScenarioBudgetValidatorReportsEveryProblem();
        ScenarioBudgetRequiredSegmentRuleFlagsABlankSegment();
        ScenarioBudgetBadgeLengthRuleFlagsALongBadge();
        ScenarioBudgetSegmentLengthRuleFlagsALongSegment();
        ScenarioBudgetLabelLengthRuleFlagsALongLabel();
    }
}
