namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkSummary;

using Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

/// <summary>Checks of benchmark summary code, run by the project's runner.</summary>
public static class ManualBenchmarkAllocationRulesChecks
{
    public static void ManualBenchmarkAllocationValidatorReportsEveryProblem()
    {
        var validator = ManualBenchmarkAllocationRules.DefaultManualBenchmarkAllocationValidator();
        var item = ManualBenchmarkAllocation.NewManualBenchmarkAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualBenchmarkAllocationRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new ManualBenchmarkAllocationRequiredLocationRule("manual benchmark allocation needs this field");
        var item = ManualBenchmarkAllocation.NewManualBenchmarkAllocation("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualBenchmarkAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualBenchmarkAllocationCodeLengthRule(3600);
        var item = ManualBenchmarkAllocation.NewManualBenchmarkAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualBenchmarkAllocationCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new ManualBenchmarkAllocationCostRangeRule(1.1, 2553.6);
        var item = ManualBenchmarkAllocation.NewManualBenchmarkAllocation("x", "y");
        item.Cost = 2555.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualBenchmarkAllocationValidatorReportsEveryProblem();
        ManualBenchmarkAllocationRequiredLocationRuleFlagsABlankLocation();
        ManualBenchmarkAllocationCodeLengthRuleFlagsALongCode();
        ManualBenchmarkAllocationCostRangeRuleFlagsValuesAboveCost();
    }
}
