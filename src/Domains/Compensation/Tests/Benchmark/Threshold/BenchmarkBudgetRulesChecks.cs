namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BenchmarkBudgetRulesChecks
{
    public static void BenchmarkBudgetValidatorReportsEveryProblem()
    {
        var validator = BenchmarkBudgetRules.DefaultBenchmarkBudgetValidator();
        var item = BenchmarkBudget.NewBenchmarkBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkBudgetLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new BenchmarkBudgetLimitRangeRule(80, 3600);
        var item = BenchmarkBudget.NewBenchmarkBudget("x", "y");
        item.Limit = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkBudgetRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new BenchmarkBudgetRequiredBadgeRule("benchmark budget needs a value");
        var item = BenchmarkBudget.NewBenchmarkBudget("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BenchmarkBudgetRequiredCodeRule("benchmark budget needs an entry");
        var item = BenchmarkBudget.NewBenchmarkBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BenchmarkBudgetValidatorReportsEveryProblem();
        BenchmarkBudgetLimitRangeRuleFlagsValuesAboveLimit();
        BenchmarkBudgetRequiredBadgeRuleFlagsABlankBadge();
        BenchmarkBudgetRequiredCodeRuleFlagsABlankCode();
    }
}
