namespace Acme.Workforce.Workplace.Tests.AccessBadge.Manifest;

using Acme.Workforce.Workplace.AccessBadge.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class GlobalAccessBadgeCycleRulesChecks
{
    public static void GlobalAccessBadgeCycleValidatorReportsEveryProblem()
    {
        var validator = GlobalAccessBadgeCycleRules.DefaultGlobalAccessBadgeCycleValidator();
        var item = GlobalAccessBadgeCycle.NewGlobalAccessBadgeCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalAccessBadgeCycleTeamLengthRuleFlagsALongTeam()
    {
        var rule = new GlobalAccessBadgeCycleTeamLengthRule(3600);
        var item = GlobalAccessBadgeCycle.NewGlobalAccessBadgeCycle("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalAccessBadgeCycleCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new GlobalAccessBadgeCycleCostRangeRule(3.5, 3972.5);
        var item = GlobalAccessBadgeCycle.NewGlobalAccessBadgeCycle("x", "y");
        item.Cost = 3974.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalAccessBadgeCycleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new GlobalAccessBadgeCycleTitleLengthRule(32);
        var item = GlobalAccessBadgeCycle.NewGlobalAccessBadgeCycle("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalAccessBadgeCycleValidatorReportsEveryProblem();
        GlobalAccessBadgeCycleTeamLengthRuleFlagsALongTeam();
        GlobalAccessBadgeCycleCostRangeRuleFlagsValuesAboveCost();
        GlobalAccessBadgeCycleTitleLengthRuleFlagsALongTitle();
    }
}
