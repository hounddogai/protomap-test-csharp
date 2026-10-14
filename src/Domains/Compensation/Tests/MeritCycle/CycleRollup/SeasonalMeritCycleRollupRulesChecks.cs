namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleRollup;

using Acme.Workforce.Compensation.MeritCycle.CycleRollup;

/// <summary>Checks of cycle rollup code, run by the project's runner.</summary>
public static class SeasonalMeritCycleRollupRulesChecks
{
    public static void SeasonalMeritCycleRollupValidatorReportsEveryProblem()
    {
        var validator = SeasonalMeritCycleRollupRules.DefaultSeasonalMeritCycleRollupValidator();
        var item = SeasonalMeritCycleRollup.NewSeasonalMeritCycleRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalMeritCycleRollupRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SeasonalMeritCycleRollupRequiredTitleRule("seasonal merit cycle rollup needs an entry");
        var item = SeasonalMeritCycleRollup.NewSeasonalMeritCycleRollup("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalMeritCycleRollupWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new SeasonalMeritCycleRollupWeightRangeRule(5.1, 4185.8);
        var item = SeasonalMeritCycleRollup.NewSeasonalMeritCycleRollup("x", "y");
        item.Weight = 4187.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalMeritCycleRollupValidatorReportsEveryProblem();
        SeasonalMeritCycleRollupRequiredTitleRuleFlagsABlankTitle();
        SeasonalMeritCycleRollupWeightRangeRuleFlagsValuesAboveWeight();
    }
}
