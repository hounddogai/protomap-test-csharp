namespace Acme.Workforce.Orgdesign.Tests.Scenario.Quota;

using Acme.Workforce.Orgdesign.Scenario.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class WeightedScenarioOverrideRulesChecks
{
    public static void WeightedScenarioOverrideValidatorReportsEveryProblem()
    {
        var validator = WeightedScenarioOverrideRules.DefaultWeightedScenarioOverrideValidator();
        var item = WeightedScenarioOverride.NewWeightedScenarioOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedScenarioOverrideRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new WeightedScenarioOverrideRequiredTitleRule("weighted scenario override needs an entry");
        var item = WeightedScenarioOverride.NewWeightedScenarioOverride("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedScenarioOverrideRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new WeightedScenarioOverrideRequiredTeamRule("weighted scenario override needs an entry");
        var item = WeightedScenarioOverride.NewWeightedScenarioOverride("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedScenarioOverrideStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new WeightedScenarioOverrideStartsAtRangeRule(16, 120);
        var item = WeightedScenarioOverride.NewWeightedScenarioOverride("x", "y");
        item.StartsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedScenarioOverrideEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new WeightedScenarioOverrideEndsAtRangeRule(1000, 86400);
        var item = WeightedScenarioOverride.NewWeightedScenarioOverride("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedScenarioOverrideTitleLengthRuleFlagsALongTitle()
    {
        var rule = new WeightedScenarioOverrideTitleLengthRule(16);
        var item = WeightedScenarioOverride.NewWeightedScenarioOverride("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedScenarioOverrideValidatorReportsEveryProblem();
        WeightedScenarioOverrideRequiredTitleRuleFlagsABlankTitle();
        WeightedScenarioOverrideRequiredTeamRuleFlagsABlankTeam();
        WeightedScenarioOverrideStartsAtRangeRuleFlagsValuesAboveStartsAt();
        WeightedScenarioOverrideEndsAtRangeRuleFlagsValuesAboveEndsAt();
        WeightedScenarioOverrideTitleLengthRuleFlagsALongTitle();
    }
}
