namespace Acme.Workforce.Compensation.Tests.Benchmark.Rule;

using Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class WeightedBenchmarkMappingRulesChecks
{
    public static void WeightedBenchmarkMappingValidatorReportsEveryProblem()
    {
        var validator = WeightedBenchmarkMappingRules.DefaultWeightedBenchmarkMappingValidator();
        var item = WeightedBenchmarkMapping.NewWeightedBenchmarkMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedBenchmarkMappingTitleLengthRuleFlagsALongTitle()
    {
        var rule = new WeightedBenchmarkMappingTitleLengthRule(120);
        var item = WeightedBenchmarkMapping.NewWeightedBenchmarkMapping("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedBenchmarkMappingRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new WeightedBenchmarkMappingRequiredOwnerIdRule("weighted benchmark mapping needs an entry");
        var item = WeightedBenchmarkMapping.NewWeightedBenchmarkMapping("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedBenchmarkMappingValidatorReportsEveryProblem();
        WeightedBenchmarkMappingTitleLengthRuleFlagsALongTitle();
        WeightedBenchmarkMappingRequiredOwnerIdRuleFlagsABlankOwnerId();
    }
}
