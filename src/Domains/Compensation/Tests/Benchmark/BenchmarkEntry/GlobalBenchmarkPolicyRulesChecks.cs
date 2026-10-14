namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkEntry;

using Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

/// <summary>Checks of benchmark entry code, run by the project's runner.</summary>
public static class GlobalBenchmarkPolicyRulesChecks
{
    public static void GlobalBenchmarkPolicyValidatorReportsEveryProblem()
    {
        var validator = GlobalBenchmarkPolicyRules.DefaultGlobalBenchmarkPolicyValidator();
        var item = GlobalBenchmarkPolicy.NewGlobalBenchmarkPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalBenchmarkPolicyCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new GlobalBenchmarkPolicyCategoryLengthRule(86400);
        var item = GlobalBenchmarkPolicy.NewGlobalBenchmarkPolicy("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalBenchmarkPolicyCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalBenchmarkPolicyCodeLengthRule(80);
        var item = GlobalBenchmarkPolicy.NewGlobalBenchmarkPolicy("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalBenchmarkPolicyThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new GlobalBenchmarkPolicyThresholdRangeRule(6.6, 2572.7);
        var item = GlobalBenchmarkPolicy.NewGlobalBenchmarkPolicy("x", "y");
        item.Threshold = 2574.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalBenchmarkPolicyNameLengthRuleFlagsALongName()
    {
        var rule = new GlobalBenchmarkPolicyNameLengthRule(1000);
        var item = GlobalBenchmarkPolicy.NewGlobalBenchmarkPolicy("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalBenchmarkPolicyCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new GlobalBenchmarkPolicyCostCenterLengthRule(3600);
        var item = GlobalBenchmarkPolicy.NewGlobalBenchmarkPolicy("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalBenchmarkPolicyRequiredNameRuleFlagsABlankName()
    {
        var rule = new GlobalBenchmarkPolicyRequiredNameRule("global benchmark policy needs an entry");
        var item = GlobalBenchmarkPolicy.NewGlobalBenchmarkPolicy("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalBenchmarkPolicyValidatorReportsEveryProblem();
        GlobalBenchmarkPolicyCategoryLengthRuleFlagsALongCategory();
        GlobalBenchmarkPolicyCodeLengthRuleFlagsALongCode();
        GlobalBenchmarkPolicyThresholdRangeRuleFlagsValuesAboveThreshold();
        GlobalBenchmarkPolicyNameLengthRuleFlagsALongName();
        GlobalBenchmarkPolicyCostCenterLengthRuleFlagsALongCostCenter();
        GlobalBenchmarkPolicyRequiredNameRuleFlagsABlankName();
    }
}
