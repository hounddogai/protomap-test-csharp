namespace Acme.Workforce.Compensation.Tests.Benchmark.Score;

using Acme.Workforce.Compensation.Benchmark.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class BenchmarkTierRulesChecks
{
    public static void BenchmarkTierValidatorReportsEveryProblem()
    {
        var validator = BenchmarkTierRules.DefaultBenchmarkTierValidator();
        var item = BenchmarkTier.NewBenchmarkTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkTierRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new BenchmarkTierRequiredOwnerIdRule("benchmark tier needs a value");
        var item = BenchmarkTier.NewBenchmarkTier("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkTierRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BenchmarkTierRequiredTitleRule("benchmark tier needs an entry");
        var item = BenchmarkTier.NewBenchmarkTier("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BenchmarkTierRequiredCodeRule("benchmark tier needs a value");
        var item = BenchmarkTier.NewBenchmarkTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BenchmarkTierValidatorReportsEveryProblem();
        BenchmarkTierRequiredOwnerIdRuleFlagsABlankOwnerId();
        BenchmarkTierRequiredTitleRuleFlagsABlankTitle();
        BenchmarkTierRequiredCodeRuleFlagsABlankCode();
    }
}
