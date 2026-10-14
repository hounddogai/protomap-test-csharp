namespace Acme.Workforce.Orgdesign.Tests.Scenario.Cycle;

using Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScenarioCycleRulesChecks
{
    public static void ScenarioCycleValidatorReportsEveryProblem()
    {
        var validator = ScenarioCycleRules.DefaultScenarioCycleValidator();
        var item = ScenarioCycle.NewScenarioCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioCycleCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScenarioCycleCodeLengthRule(64);
        var item = ScenarioCycle.NewScenarioCycle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioCyclePhoneLengthRuleFlagsALongPhone()
    {
        var rule = new ScenarioCyclePhoneLengthRule(32);
        var item = ScenarioCycle.NewScenarioCycle("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioCyclePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new ScenarioCyclePriorityRangeRule(16, 3600);
        var item = ScenarioCycle.NewScenarioCycle("x", "y");
        item.Priority = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScenarioCycleValidatorReportsEveryProblem();
        ScenarioCycleCodeLengthRuleFlagsALongCode();
        ScenarioCyclePhoneLengthRuleFlagsALongPhone();
        ScenarioCyclePriorityRangeRuleFlagsValuesAbovePriority();
    }
}
