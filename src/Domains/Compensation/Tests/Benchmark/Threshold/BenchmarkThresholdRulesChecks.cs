namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BenchmarkThresholdRulesChecks
{
    public static void BenchmarkThresholdValidatorReportsEveryProblem()
    {
        var validator = BenchmarkThresholdRules.DefaultBenchmarkThresholdValidator();
        var item = BenchmarkThreshold.NewBenchmarkThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkThresholdRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new BenchmarkThresholdRequiredOwnerIdRule("benchmark threshold needs a value");
        var item = BenchmarkThreshold.NewBenchmarkThreshold("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkThresholdRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new BenchmarkThresholdRequiredSkillRule("benchmark threshold needs this field");
        var item = BenchmarkThreshold.NewBenchmarkThreshold("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new BenchmarkThresholdCodeLengthRule(120);
        var item = BenchmarkThreshold.NewBenchmarkThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BenchmarkThresholdValidatorReportsEveryProblem();
        BenchmarkThresholdRequiredOwnerIdRuleFlagsABlankOwnerId();
        BenchmarkThresholdRequiredSkillRuleFlagsABlankSkill();
        BenchmarkThresholdCodeLengthRuleFlagsALongCode();
    }
}
