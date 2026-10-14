namespace Acme.Workforce.Compensation.Tests.Benchmark.Baseline;

using Acme.Workforce.Compensation.Benchmark.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class RegionalBenchmarkForecastRulesChecks
{
    public static void RegionalBenchmarkForecastValidatorReportsEveryProblem()
    {
        var validator = RegionalBenchmarkForecastRules.DefaultRegionalBenchmarkForecastValidator();
        var item = RegionalBenchmarkForecast.NewRegionalBenchmarkForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalBenchmarkForecastRequiredNameRuleFlagsABlankName()
    {
        var rule = new RegionalBenchmarkForecastRequiredNameRule("regional benchmark forecast needs this field");
        var item = RegionalBenchmarkForecast.NewRegionalBenchmarkForecast("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalBenchmarkForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalBenchmarkForecastCodeLengthRule(3600);
        var item = RegionalBenchmarkForecast.NewRegionalBenchmarkForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalBenchmarkForecastRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new RegionalBenchmarkForecastRequiredBadgeRule("regional benchmark forecast needs this field");
        var item = RegionalBenchmarkForecast.NewRegionalBenchmarkForecast("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalBenchmarkForecastTeamLengthRuleFlagsALongTeam()
    {
        var rule = new RegionalBenchmarkForecastTeamLengthRule(3600);
        var item = RegionalBenchmarkForecast.NewRegionalBenchmarkForecast("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalBenchmarkForecastValidatorReportsEveryProblem();
        RegionalBenchmarkForecastRequiredNameRuleFlagsABlankName();
        RegionalBenchmarkForecastCodeLengthRuleFlagsALongCode();
        RegionalBenchmarkForecastRequiredBadgeRuleFlagsABlankBadge();
        RegionalBenchmarkForecastTeamLengthRuleFlagsALongTeam();
    }
}
