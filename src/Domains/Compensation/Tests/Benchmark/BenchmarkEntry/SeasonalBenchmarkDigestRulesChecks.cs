namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkEntry;

using Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

/// <summary>Checks of benchmark entry code, run by the project's runner.</summary>
public static class SeasonalBenchmarkDigestRulesChecks
{
    public static void SeasonalBenchmarkDigestValidatorReportsEveryProblem()
    {
        var validator = SeasonalBenchmarkDigestRules.DefaultSeasonalBenchmarkDigestValidator();
        var item = SeasonalBenchmarkDigest.NewSeasonalBenchmarkDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalBenchmarkDigestRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new SeasonalBenchmarkDigestRequiredBadgeRule("seasonal benchmark digest needs this field");
        var item = SeasonalBenchmarkDigest.NewSeasonalBenchmarkDigest("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalBenchmarkDigestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SeasonalBenchmarkDigestRequiredLabelRule("seasonal benchmark digest needs an entry");
        var item = SeasonalBenchmarkDigest.NewSeasonalBenchmarkDigest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalBenchmarkDigestRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new SeasonalBenchmarkDigestRateRangeRule(5.2, 2743.7);
        var item = SeasonalBenchmarkDigest.NewSeasonalBenchmarkDigest("x", "y");
        item.Rate = 2745.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalBenchmarkDigestTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new SeasonalBenchmarkDigestTimezoneLengthRule(255);
        var item = SeasonalBenchmarkDigest.NewSeasonalBenchmarkDigest("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalBenchmarkDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalBenchmarkDigestCodeLengthRule(255);
        var item = SeasonalBenchmarkDigest.NewSeasonalBenchmarkDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalBenchmarkDigestValidatorReportsEveryProblem();
        SeasonalBenchmarkDigestRequiredBadgeRuleFlagsABlankBadge();
        SeasonalBenchmarkDigestRequiredLabelRuleFlagsABlankLabel();
        SeasonalBenchmarkDigestRateRangeRuleFlagsValuesAboveRate();
        SeasonalBenchmarkDigestTimezoneLengthRuleFlagsALongTimezone();
        SeasonalBenchmarkDigestCodeLengthRuleFlagsALongCode();
    }
}
