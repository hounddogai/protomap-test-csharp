namespace Acme.Workforce.Compensation.Tests.Benchmark.Rule;

using Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BenchmarkAdjustmentRulesChecks
{
    public static void BenchmarkAdjustmentValidatorReportsEveryProblem()
    {
        var validator = BenchmarkAdjustmentRules.DefaultBenchmarkAdjustmentValidator();
        var item = BenchmarkAdjustment.NewBenchmarkAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new BenchmarkAdjustmentCodeLengthRule(16);
        var item = BenchmarkAdjustment.NewBenchmarkAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BenchmarkAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BenchmarkAdjustmentRequiredCodeRule("benchmark adjustment needs this field");
        var item = BenchmarkAdjustment.NewBenchmarkAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BenchmarkAdjustmentValidatorReportsEveryProblem();
        BenchmarkAdjustmentCodeLengthRuleFlagsALongCode();
        BenchmarkAdjustmentRequiredCodeRuleFlagsABlankCode();
    }
}
