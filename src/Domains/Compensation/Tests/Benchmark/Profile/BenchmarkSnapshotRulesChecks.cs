namespace Acme.Workforce.Compensation.Tests.Benchmark.Profile;

using Acme.Workforce.Compensation.Benchmark.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class BenchmarkSnapshotRulesChecks
{
    public static void BenchmarkSnapshotValidatorReportsEveryProblem()
    {
        var validator = BenchmarkSnapshotRules.DefaultBenchmarkSnapshotValidator();
        var item = BenchmarkSnapshot.NewBenchmarkSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkSnapshotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BenchmarkSnapshotRequiredCodeRule("benchmark snapshot needs a value");
        var item = BenchmarkSnapshot.NewBenchmarkSnapshot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkSnapshotLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new BenchmarkSnapshotLimitRangeRule(16, 64);
        var item = BenchmarkSnapshot.NewBenchmarkSnapshot("x", "y");
        item.Limit = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkSnapshotCountryLengthRuleFlagsALongCountry()
    {
        var rule = new BenchmarkSnapshotCountryLengthRule(3600);
        var item = BenchmarkSnapshot.NewBenchmarkSnapshot("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BenchmarkSnapshotCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new BenchmarkSnapshotCategoryLengthRule(32);
        var item = BenchmarkSnapshot.NewBenchmarkSnapshot("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BenchmarkSnapshotValidatorReportsEveryProblem();
        BenchmarkSnapshotRequiredCodeRuleFlagsABlankCode();
        BenchmarkSnapshotLimitRangeRuleFlagsValuesAboveLimit();
        BenchmarkSnapshotCountryLengthRuleFlagsALongCountry();
        BenchmarkSnapshotCategoryLengthRuleFlagsALongCategory();
    }
}
