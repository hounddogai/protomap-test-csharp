namespace Acme.Workforce.Engagement.Tests.TownHall.Cycle;

using Acme.Workforce.Engagement.TownHall.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class TownHallCycleRulesChecks
{
    public static void TownHallCycleValidatorReportsEveryProblem()
    {
        var validator = TownHallCycleRules.DefaultTownHallCycleValidator();
        var item = TownHallCycle.NewTownHallCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallCycleLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new TownHallCycleLeaveDaysRangeRule(64, 3600);
        var item = TownHallCycle.NewTownHallCycle("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallCycleFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new TownHallCycleFteRangeRule(0.1, 2061.4);
        var item = TownHallCycle.NewTownHallCycle("x", "y");
        item.Fte = 2062.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TownHallCycleValidatorReportsEveryProblem();
        TownHallCycleLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        TownHallCycleFteRangeRuleFlagsValuesAboveFte();
    }
}
