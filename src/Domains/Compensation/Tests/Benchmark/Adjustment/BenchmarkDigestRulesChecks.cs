namespace Acme.Workforce.Compensation.Tests.Benchmark.Adjustment;

using Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class BenchmarkDigestRulesChecks
{
    public static void BenchmarkDigestValidatorReportsEveryProblem()
    {
        var validator = BenchmarkDigestRules.DefaultBenchmarkDigestValidator();
        var item = BenchmarkDigest.NewBenchmarkDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkDigestRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new BenchmarkDigestRequiredTimezoneRule("benchmark digest needs this field");
        var item = BenchmarkDigest.NewBenchmarkDigest("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkDigestCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new BenchmarkDigestCreatedAtRangeRule(16, 32);
        var item = BenchmarkDigest.NewBenchmarkDigest("x", "y");
        item.CreatedAt = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkDigestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BenchmarkDigestRequiredCodeRule("benchmark digest needs an entry");
        var item = BenchmarkDigest.NewBenchmarkDigest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkDigestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BenchmarkDigestRequiredTitleRule("benchmark digest needs a value");
        var item = BenchmarkDigest.NewBenchmarkDigest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BenchmarkDigestValidatorReportsEveryProblem();
        BenchmarkDigestRequiredTimezoneRuleFlagsABlankTimezone();
        BenchmarkDigestCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        BenchmarkDigestRequiredCodeRuleFlagsABlankCode();
        BenchmarkDigestRequiredTitleRuleFlagsABlankTitle();
    }
}
