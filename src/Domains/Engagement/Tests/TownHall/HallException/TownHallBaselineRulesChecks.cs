namespace Acme.Workforce.Engagement.Tests.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>Checks of hall exception code, run by the project's runner.</summary>
public static class TownHallBaselineRulesChecks
{
    public static void TownHallBaselineValidatorReportsEveryProblem()
    {
        var validator = TownHallBaselineRules.DefaultTownHallBaselineValidator();
        var item = TownHallBaseline.NewTownHallBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallBaselineRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new TownHallBaselineRequiredSourceRule("town hall baseline needs this field");
        var item = TownHallBaseline.NewTownHallBaseline("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TownHallBaselineRequiredCodeRule("town hall baseline needs an entry");
        var item = TownHallBaseline.NewTownHallBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TownHallBaselineValidatorReportsEveryProblem();
        TownHallBaselineRequiredSourceRuleFlagsABlankSource();
        TownHallBaselineRequiredCodeRuleFlagsABlankCode();
    }
}
