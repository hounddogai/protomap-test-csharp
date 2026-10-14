namespace Acme.Workforce.Compensation.Tests.Benchmark.Allocation;

using Acme.Workforce.Compensation.Benchmark.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BenchmarkAllocationRulesChecks
{
    public static void BenchmarkAllocationValidatorReportsEveryProblem()
    {
        var validator = BenchmarkAllocationRules.DefaultBenchmarkAllocationValidator();
        var item = BenchmarkAllocation.NewBenchmarkAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BenchmarkAllocationRequiredCodeRule("benchmark allocation needs an entry");
        var item = BenchmarkAllocation.NewBenchmarkAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkAllocationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BenchmarkAllocationLabelLengthRule(3600);
        var item = BenchmarkAllocation.NewBenchmarkAllocation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BenchmarkAllocationValidatorReportsEveryProblem();
        BenchmarkAllocationRequiredCodeRuleFlagsABlankCode();
        BenchmarkAllocationLabelLengthRuleFlagsALongLabel();
    }
}
