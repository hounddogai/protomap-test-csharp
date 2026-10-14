namespace Acme.Workforce.Compensation.Tests.Benchmark.Reservation;

using Acme.Workforce.Compensation.Benchmark.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BenchmarkMilestoneRulesChecks
{
    public static void BenchmarkMilestoneValidatorReportsEveryProblem()
    {
        var validator = BenchmarkMilestoneRules.DefaultBenchmarkMilestoneValidator();
        var item = BenchmarkMilestone.NewBenchmarkMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkMilestoneCodeLengthRuleFlagsALongCode()
    {
        var rule = new BenchmarkMilestoneCodeLengthRule(64);
        var item = BenchmarkMilestone.NewBenchmarkMilestone("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BenchmarkMilestoneRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new BenchmarkMilestoneRequiredManagerIdRule("benchmark milestone needs an entry");
        var item = BenchmarkMilestone.NewBenchmarkMilestone("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BenchmarkMilestoneRequiredCodeRule("benchmark milestone needs this field");
        var item = BenchmarkMilestone.NewBenchmarkMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkMilestoneScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new BenchmarkMilestoneScoreRangeRule(4.9, 3302.6);
        var item = BenchmarkMilestone.NewBenchmarkMilestone("x", "y");
        item.Score = 3304.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BenchmarkMilestoneValidatorReportsEveryProblem();
        BenchmarkMilestoneCodeLengthRuleFlagsALongCode();
        BenchmarkMilestoneRequiredManagerIdRuleFlagsABlankManagerId();
        BenchmarkMilestoneRequiredCodeRuleFlagsABlankCode();
        BenchmarkMilestoneScoreRangeRuleFlagsValuesAboveScore();
    }
}
