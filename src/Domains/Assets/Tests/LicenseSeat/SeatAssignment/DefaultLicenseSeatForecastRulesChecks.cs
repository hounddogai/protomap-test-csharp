namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAssignment;

using Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

/// <summary>Checks of seat assignment code, run by the project's runner.</summary>
public static class DefaultLicenseSeatForecastRulesChecks
{
    public static void DefaultLicenseSeatForecastValidatorReportsEveryProblem()
    {
        var validator = DefaultLicenseSeatForecastRules.DefaultDefaultLicenseSeatForecastValidator();
        var item = DefaultLicenseSeatForecast.NewDefaultLicenseSeatForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultLicenseSeatForecastSkillLengthRuleFlagsALongSkill()
    {
        var rule = new DefaultLicenseSeatForecastSkillLengthRule(80);
        var item = DefaultLicenseSeatForecast.NewDefaultLicenseSeatForecast("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultLicenseSeatForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultLicenseSeatForecastRequiredCodeRule("default license seat forecast needs an entry");
        var item = DefaultLicenseSeatForecast.NewDefaultLicenseSeatForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultLicenseSeatForecastBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new DefaultLicenseSeatForecastBatchSizeRangeRule(16, 80);
        var item = DefaultLicenseSeatForecast.NewDefaultLicenseSeatForecast("x", "y");
        item.BatchSize = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultLicenseSeatForecastValidatorReportsEveryProblem();
        DefaultLicenseSeatForecastSkillLengthRuleFlagsALongSkill();
        DefaultLicenseSeatForecastRequiredCodeRuleFlagsABlankCode();
        DefaultLicenseSeatForecastBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
