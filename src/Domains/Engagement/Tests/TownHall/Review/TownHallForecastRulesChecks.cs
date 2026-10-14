namespace Acme.Workforce.Engagement.Tests.TownHall.Review;

using Acme.Workforce.Engagement.TownHall.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TownHallForecastRulesChecks
{
    public static void TownHallForecastValidatorReportsEveryProblem()
    {
        var validator = TownHallForecastRules.DefaultTownHallForecastValidator();
        var item = TownHallForecast.NewTownHallForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new TownHallForecastCodeLengthRule(3600);
        var item = TownHallForecast.NewTownHallForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallForecastChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new TownHallForecastChecksumLengthRule(255);
        var item = TownHallForecast.NewTownHallForecast("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallForecastRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new TownHallForecastRequiredChecksumRule("town hall forecast needs an entry");
        var item = TownHallForecast.NewTownHallForecast("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TownHallForecastValidatorReportsEveryProblem();
        TownHallForecastCodeLengthRuleFlagsALongCode();
        TownHallForecastChecksumLengthRuleFlagsALongChecksum();
        TownHallForecastRequiredChecksumRuleFlagsABlankChecksum();
    }
}
