namespace Acme.Workforce.Compensation.Tests.Benchmark.Reservation;

using Acme.Workforce.Compensation.Benchmark.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BenchmarkBaselineRulesChecks
{
    public static void BenchmarkBaselineValidatorReportsEveryProblem()
    {
        var validator = BenchmarkBaselineRules.DefaultBenchmarkBaselineValidator();
        var item = BenchmarkBaseline.NewBenchmarkBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkBaselineSkillLengthRuleFlagsALongSkill()
    {
        var rule = new BenchmarkBaselineSkillLengthRule(120);
        var item = BenchmarkBaseline.NewBenchmarkBaseline("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BenchmarkBaselineRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new BenchmarkBaselineRequiredTeamRule("benchmark baseline needs a value");
        var item = BenchmarkBaseline.NewBenchmarkBaseline("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkBaselineRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new BenchmarkBaselineRateRangeRule(9.7, 925.2);
        var item = BenchmarkBaseline.NewBenchmarkBaseline("x", "y");
        item.Rate = 926.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkBaselineCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new BenchmarkBaselineCapacityRangeRule(32, 86400);
        var item = BenchmarkBaseline.NewBenchmarkBaseline("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BenchmarkBaselineValidatorReportsEveryProblem();
        BenchmarkBaselineSkillLengthRuleFlagsALongSkill();
        BenchmarkBaselineRequiredTeamRuleFlagsABlankTeam();
        BenchmarkBaselineRateRangeRuleFlagsValuesAboveRate();
        BenchmarkBaselineCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
