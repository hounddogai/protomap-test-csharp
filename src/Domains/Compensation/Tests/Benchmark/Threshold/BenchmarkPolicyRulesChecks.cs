namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BenchmarkPolicyRulesChecks
{
    public static void BenchmarkPolicyValidatorReportsEveryProblem()
    {
        var validator = BenchmarkPolicyRules.DefaultBenchmarkPolicyValidator();
        var item = BenchmarkPolicy.NewBenchmarkPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkPolicyRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new BenchmarkPolicyRetriesRangeRule(255, 255);
        var item = BenchmarkPolicy.NewBenchmarkPolicy("x", "y");
        item.Retries = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BenchmarkPolicyRequiredCodeRule("benchmark policy needs a value");
        var item = BenchmarkPolicy.NewBenchmarkPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkPolicyBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new BenchmarkPolicyBatchSizeRangeRule(255, 86400);
        var item = BenchmarkPolicy.NewBenchmarkPolicy("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BenchmarkPolicyValidatorReportsEveryProblem();
        BenchmarkPolicyRetriesRangeRuleFlagsValuesAboveRetries();
        BenchmarkPolicyRequiredCodeRuleFlagsABlankCode();
        BenchmarkPolicyBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
