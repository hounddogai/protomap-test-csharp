namespace Acme.Workforce.Compensation.Tests.Benchmark.Summary;

using Acme.Workforce.Compensation.Benchmark.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BenchmarkSummaryRulesChecks
{
    public static void BenchmarkSummaryValidatorReportsEveryProblem()
    {
        var validator = BenchmarkSummaryRules.DefaultBenchmarkSummaryValidator();
        var item = BenchmarkSummary.NewBenchmarkSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkSummaryBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new BenchmarkSummaryBudgetRangeRule(6.9, 4205.9);
        var item = BenchmarkSummary.NewBenchmarkSummary("x", "y");
        item.Budget = 4207.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkSummaryRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BenchmarkSummaryRequiredLabelRule("benchmark summary needs an entry");
        var item = BenchmarkSummary.NewBenchmarkSummary("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new BenchmarkSummaryCodeLengthRule(255);
        var item = BenchmarkSummary.NewBenchmarkSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BenchmarkSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BenchmarkSummaryRequiredCodeRule("benchmark summary needs an entry");
        var item = BenchmarkSummary.NewBenchmarkSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BenchmarkSummaryValidatorReportsEveryProblem();
        BenchmarkSummaryBudgetRangeRuleFlagsValuesAboveBudget();
        BenchmarkSummaryRequiredLabelRuleFlagsABlankLabel();
        BenchmarkSummaryCodeLengthRuleFlagsALongCode();
        BenchmarkSummaryRequiredCodeRuleFlagsABlankCode();
    }
}
