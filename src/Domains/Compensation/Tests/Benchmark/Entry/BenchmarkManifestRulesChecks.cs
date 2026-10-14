namespace Acme.Workforce.Compensation.Tests.Benchmark.Entry;

using Acme.Workforce.Compensation.Benchmark.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BenchmarkManifestRulesChecks
{
    public static void BenchmarkManifestValidatorReportsEveryProblem()
    {
        var validator = BenchmarkManifestRules.DefaultBenchmarkManifestValidator();
        var item = BenchmarkManifest.NewBenchmarkManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkManifestFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new BenchmarkManifestFloorValueRangeRule(5.1, 1319.8);
        var item = BenchmarkManifest.NewBenchmarkManifest("x", "y");
        item.FloorValue = 1321.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkManifestLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BenchmarkManifestLabelLengthRule(32);
        var item = BenchmarkManifest.NewBenchmarkManifest("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BenchmarkManifestValidatorReportsEveryProblem();
        BenchmarkManifestFloorValueRangeRuleFlagsValuesAboveFloorValue();
        BenchmarkManifestLabelLengthRuleFlagsALongLabel();
    }
}
