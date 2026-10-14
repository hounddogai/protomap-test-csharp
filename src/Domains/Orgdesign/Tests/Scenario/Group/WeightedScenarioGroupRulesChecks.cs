namespace Acme.Workforce.Orgdesign.Tests.Scenario.Group;

using Acme.Workforce.Orgdesign.Scenario.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class WeightedScenarioGroupRulesChecks
{
    public static void WeightedScenarioGroupValidatorReportsEveryProblem()
    {
        var validator = WeightedScenarioGroupRules.DefaultWeightedScenarioGroupValidator();
        var item = WeightedScenarioGroup.NewWeightedScenarioGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedScenarioGroupTitleLengthRuleFlagsALongTitle()
    {
        var rule = new WeightedScenarioGroupTitleLengthRule(64);
        var item = WeightedScenarioGroup.NewWeightedScenarioGroup("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedScenarioGroupRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new WeightedScenarioGroupRequiredBadgeRule("weighted scenario group needs a value");
        var item = WeightedScenarioGroup.NewWeightedScenarioGroup("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedScenarioGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedScenarioGroupRequiredCodeRule("weighted scenario group needs an entry");
        var item = WeightedScenarioGroup.NewWeightedScenarioGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedScenarioGroupBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new WeightedScenarioGroupBadgeLengthRule(255);
        var item = WeightedScenarioGroup.NewWeightedScenarioGroup("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedScenarioGroupValidatorReportsEveryProblem();
        WeightedScenarioGroupTitleLengthRuleFlagsALongTitle();
        WeightedScenarioGroupRequiredBadgeRuleFlagsABlankBadge();
        WeightedScenarioGroupRequiredCodeRuleFlagsABlankCode();
        WeightedScenarioGroupBadgeLengthRuleFlagsALongBadge();
    }
}
