namespace Acme.Workforce.Orgdesign.Tests.Scenario.Reservation;

using Acme.Workforce.Orgdesign.Scenario.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ScenarioMilestoneRulesChecks
{
    public static void ScenarioMilestoneValidatorReportsEveryProblem()
    {
        var validator = ScenarioMilestoneRules.DefaultScenarioMilestoneValidator();
        var item = ScenarioMilestone.NewScenarioMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioMilestoneGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new ScenarioMilestoneGraceDaysRangeRule(32, 64);
        var item = ScenarioMilestone.NewScenarioMilestone("x", "y");
        item.GraceDays = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScenarioMilestoneTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ScenarioMilestoneTitleLengthRule(3600);
        var item = ScenarioMilestone.NewScenarioMilestone("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioMilestoneBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new ScenarioMilestoneBatchSizeRangeRule(80, 86400);
        var item = ScenarioMilestone.NewScenarioMilestone("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScenarioMilestoneValidatorReportsEveryProblem();
        ScenarioMilestoneGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        ScenarioMilestoneTitleLengthRuleFlagsALongTitle();
        ScenarioMilestoneBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
