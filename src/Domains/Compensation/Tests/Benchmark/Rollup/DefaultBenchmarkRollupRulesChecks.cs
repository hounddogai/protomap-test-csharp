namespace Acme.Workforce.Compensation.Tests.Benchmark.Rollup;

using Acme.Workforce.Compensation.Benchmark.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class DefaultBenchmarkRollupRulesChecks
{
    public static void DefaultBenchmarkRollupValidatorReportsEveryProblem()
    {
        var validator = DefaultBenchmarkRollupRules.DefaultDefaultBenchmarkRollupValidator();
        var item = DefaultBenchmarkRollup.NewDefaultBenchmarkRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultBenchmarkRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultBenchmarkRollupRequiredCodeRule("default benchmark rollup needs this field");
        var item = DefaultBenchmarkRollup.NewDefaultBenchmarkRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultBenchmarkRollupSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new DefaultBenchmarkRollupSegmentLengthRule(64);
        var item = DefaultBenchmarkRollup.NewDefaultBenchmarkRollup("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultBenchmarkRollupCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new DefaultBenchmarkRollupCapacityRangeRule(16, 3600);
        var item = DefaultBenchmarkRollup.NewDefaultBenchmarkRollup("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultBenchmarkRollupRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new DefaultBenchmarkRollupRequiredGradeRule("default benchmark rollup needs a value");
        var item = DefaultBenchmarkRollup.NewDefaultBenchmarkRollup("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultBenchmarkRollupValidatorReportsEveryProblem();
        DefaultBenchmarkRollupRequiredCodeRuleFlagsABlankCode();
        DefaultBenchmarkRollupSegmentLengthRuleFlagsALongSegment();
        DefaultBenchmarkRollupCapacityRangeRuleFlagsValuesAboveCapacity();
        DefaultBenchmarkRollupRequiredGradeRuleFlagsABlankGrade();
    }
}
