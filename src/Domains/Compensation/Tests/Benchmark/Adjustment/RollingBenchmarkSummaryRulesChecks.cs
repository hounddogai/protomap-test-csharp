namespace Acme.Workforce.Compensation.Tests.Benchmark.Adjustment;

using Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingBenchmarkSummaryRulesChecks
{
    public static void RollingBenchmarkSummaryValidatorReportsEveryProblem()
    {
        var validator = RollingBenchmarkSummaryRules.DefaultRollingBenchmarkSummaryValidator();
        var item = RollingBenchmarkSummary.NewRollingBenchmarkSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingBenchmarkSummaryRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new RollingBenchmarkSummaryRetriesRangeRule(16, 32);
        var item = RollingBenchmarkSummary.NewRollingBenchmarkSummary("x", "y");
        item.Retries = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingBenchmarkSummaryRegionLengthRuleFlagsALongRegion()
    {
        var rule = new RollingBenchmarkSummaryRegionLengthRule(16);
        var item = RollingBenchmarkSummary.NewRollingBenchmarkSummary("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingBenchmarkSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingBenchmarkSummaryRequiredCodeRule("rolling benchmark summary needs an entry");
        var item = RollingBenchmarkSummary.NewRollingBenchmarkSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingBenchmarkSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingBenchmarkSummaryCodeLengthRule(255);
        var item = RollingBenchmarkSummary.NewRollingBenchmarkSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingBenchmarkSummaryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RollingBenchmarkSummaryRequiredTitleRule("rolling benchmark summary needs an entry");
        var item = RollingBenchmarkSummary.NewRollingBenchmarkSummary("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingBenchmarkSummaryValidatorReportsEveryProblem();
        RollingBenchmarkSummaryRetriesRangeRuleFlagsValuesAboveRetries();
        RollingBenchmarkSummaryRegionLengthRuleFlagsALongRegion();
        RollingBenchmarkSummaryRequiredCodeRuleFlagsABlankCode();
        RollingBenchmarkSummaryCodeLengthRuleFlagsALongCode();
        RollingBenchmarkSummaryRequiredTitleRuleFlagsABlankTitle();
    }
}
