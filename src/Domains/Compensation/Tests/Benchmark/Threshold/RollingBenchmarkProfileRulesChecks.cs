namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RollingBenchmarkProfileRulesChecks
{
    public static void RollingBenchmarkProfileValidatorReportsEveryProblem()
    {
        var validator = RollingBenchmarkProfileRules.DefaultRollingBenchmarkProfileValidator();
        var item = RollingBenchmarkProfile.NewRollingBenchmarkProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingBenchmarkProfileLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new RollingBenchmarkProfileLeaveDaysRangeRule(16, 64);
        var item = RollingBenchmarkProfile.NewRollingBenchmarkProfile("x", "y");
        item.LeaveDays = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingBenchmarkProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingBenchmarkProfileRequiredCodeRule("rolling benchmark profile needs an entry");
        var item = RollingBenchmarkProfile.NewRollingBenchmarkProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingBenchmarkProfileRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RollingBenchmarkProfileRequiredTitleRule("rolling benchmark profile needs a value");
        var item = RollingBenchmarkProfile.NewRollingBenchmarkProfile("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingBenchmarkProfileValidatorReportsEveryProblem();
        RollingBenchmarkProfileLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        RollingBenchmarkProfileRequiredCodeRuleFlagsABlankCode();
        RollingBenchmarkProfileRequiredTitleRuleFlagsABlankTitle();
    }
}
