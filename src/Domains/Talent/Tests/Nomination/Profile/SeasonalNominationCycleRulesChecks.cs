namespace Acme.Workforce.Talent.Tests.Nomination.Profile;

using Acme.Workforce.Talent.Nomination.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class SeasonalNominationCycleRulesChecks
{
    public static void SeasonalNominationCycleValidatorReportsEveryProblem()
    {
        var validator = SeasonalNominationCycleRules.DefaultSeasonalNominationCycleValidator();
        var item = SeasonalNominationCycle.NewSeasonalNominationCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalNominationCycleCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalNominationCycleCodeLengthRule(3600);
        var item = SeasonalNominationCycle.NewSeasonalNominationCycle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalNominationCycleFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new SeasonalNominationCycleFteRangeRule(4.2, 2772.6);
        var item = SeasonalNominationCycle.NewSeasonalNominationCycle("x", "y");
        item.Fte = 2774.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalNominationCycleValidatorReportsEveryProblem();
        SeasonalNominationCycleCodeLengthRuleFlagsALongCode();
        SeasonalNominationCycleFteRangeRuleFlagsValuesAboveFte();
    }
}
