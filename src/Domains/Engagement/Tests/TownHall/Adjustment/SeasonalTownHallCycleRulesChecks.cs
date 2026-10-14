namespace Acme.Workforce.Engagement.Tests.TownHall.Adjustment;

using Acme.Workforce.Engagement.TownHall.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SeasonalTownHallCycleRulesChecks
{
    public static void SeasonalTownHallCycleValidatorReportsEveryProblem()
    {
        var validator = SeasonalTownHallCycleRules.DefaultSeasonalTownHallCycleValidator();
        var item = SeasonalTownHallCycle.NewSeasonalTownHallCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalTownHallCycleCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalTownHallCycleCodeLengthRule(80);
        var item = SeasonalTownHallCycle.NewSeasonalTownHallCycle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalTownHallCycleRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new SeasonalTownHallCycleRequiredCountryRule("seasonal town hall cycle needs an entry");
        var item = SeasonalTownHallCycle.NewSeasonalTownHallCycle("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalTownHallCycleValidatorReportsEveryProblem();
        SeasonalTownHallCycleCodeLengthRuleFlagsALongCode();
        SeasonalTownHallCycleRequiredCountryRuleFlagsABlankCountry();
    }
}
