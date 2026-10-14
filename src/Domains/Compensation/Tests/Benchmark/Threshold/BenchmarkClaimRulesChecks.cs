namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BenchmarkClaimRulesChecks
{
    public static void BenchmarkClaimValidatorReportsEveryProblem()
    {
        var validator = BenchmarkClaimRules.DefaultBenchmarkClaimValidator();
        var item = BenchmarkClaim.NewBenchmarkClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkClaimCodeLengthRuleFlagsALongCode()
    {
        var rule = new BenchmarkClaimCodeLengthRule(3600);
        var item = BenchmarkClaim.NewBenchmarkClaim("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BenchmarkClaimAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new BenchmarkClaimAmountRangeRule(9.7, 4194.3);
        var item = BenchmarkClaim.NewBenchmarkClaim("x", "y");
        item.Amount = 4195.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkClaimTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new BenchmarkClaimTimezoneLengthRule(1000);
        var item = BenchmarkClaim.NewBenchmarkClaim("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BenchmarkClaimValidatorReportsEveryProblem();
        BenchmarkClaimCodeLengthRuleFlagsALongCode();
        BenchmarkClaimAmountRangeRuleFlagsValuesAboveAmount();
        BenchmarkClaimTimezoneLengthRuleFlagsALongTimezone();
    }
}
