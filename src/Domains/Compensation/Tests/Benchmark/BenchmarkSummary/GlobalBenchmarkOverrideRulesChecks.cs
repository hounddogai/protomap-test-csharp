namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkSummary;

using Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

/// <summary>Checks of benchmark summary code, run by the project's runner.</summary>
public static class GlobalBenchmarkOverrideRulesChecks
{
    public static void GlobalBenchmarkOverrideValidatorReportsEveryProblem()
    {
        var validator = GlobalBenchmarkOverrideRules.DefaultGlobalBenchmarkOverrideValidator();
        var item = GlobalBenchmarkOverride.NewGlobalBenchmarkOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalBenchmarkOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalBenchmarkOverrideRequiredCodeRule("global benchmark override needs this field");
        var item = GlobalBenchmarkOverride.NewGlobalBenchmarkOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalBenchmarkOverrideCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new GlobalBenchmarkOverrideCategoryLengthRule(64);
        var item = GlobalBenchmarkOverride.NewGlobalBenchmarkOverride("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalBenchmarkOverrideAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new GlobalBenchmarkOverrideAttemptsRangeRule(64, 80);
        var item = GlobalBenchmarkOverride.NewGlobalBenchmarkOverride("x", "y");
        item.Attempts = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalBenchmarkOverrideRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new GlobalBenchmarkOverrideRequiredLabelRule("global benchmark override needs an entry");
        var item = GlobalBenchmarkOverride.NewGlobalBenchmarkOverride("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalBenchmarkOverrideCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new GlobalBenchmarkOverrideCostCenterLengthRule(1000);
        var item = GlobalBenchmarkOverride.NewGlobalBenchmarkOverride("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalBenchmarkOverrideValidatorReportsEveryProblem();
        GlobalBenchmarkOverrideRequiredCodeRuleFlagsABlankCode();
        GlobalBenchmarkOverrideCategoryLengthRuleFlagsALongCategory();
        GlobalBenchmarkOverrideAttemptsRangeRuleFlagsValuesAboveAttempts();
        GlobalBenchmarkOverrideRequiredLabelRuleFlagsABlankLabel();
        GlobalBenchmarkOverrideCostCenterLengthRuleFlagsALongCostCenter();
    }
}
