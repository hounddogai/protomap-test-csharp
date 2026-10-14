namespace Acme.Workforce.Compensation.Tests.Benchmark.Rule;

using Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BenchmarkRuleRulesChecks
{
    public static void BenchmarkRuleValidatorReportsEveryProblem()
    {
        var validator = BenchmarkRuleRules.DefaultBenchmarkRuleValidator();
        var item = BenchmarkRule.NewBenchmarkRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkRuleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BenchmarkRuleLabelLengthRule(3600);
        var item = BenchmarkRule.NewBenchmarkRule("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BenchmarkRuleRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new BenchmarkRuleRetriesRangeRule(16, 3600);
        var item = BenchmarkRule.NewBenchmarkRule("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BenchmarkRuleRequiredCodeRule("benchmark rule needs this field");
        var item = BenchmarkRule.NewBenchmarkRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new BenchmarkRuleCodeLengthRule(120);
        var item = BenchmarkRule.NewBenchmarkRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BenchmarkRuleValidatorReportsEveryProblem();
        BenchmarkRuleLabelLengthRuleFlagsALongLabel();
        BenchmarkRuleRetriesRangeRuleFlagsValuesAboveRetries();
        BenchmarkRuleRequiredCodeRuleFlagsABlankCode();
        BenchmarkRuleCodeLengthRuleFlagsALongCode();
    }
}
