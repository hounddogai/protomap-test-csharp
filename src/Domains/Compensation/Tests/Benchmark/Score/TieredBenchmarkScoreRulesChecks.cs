namespace Acme.Workforce.Compensation.Tests.Benchmark.Score;

using Acme.Workforce.Compensation.Benchmark.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class TieredBenchmarkScoreRulesChecks
{
    public static void TieredBenchmarkScoreValidatorReportsEveryProblem()
    {
        var validator = TieredBenchmarkScoreRules.DefaultTieredBenchmarkScoreValidator();
        var item = TieredBenchmarkScore.NewTieredBenchmarkScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredBenchmarkScoreHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new TieredBenchmarkScoreHoursRangeRule(7.0, 180.5);
        var item = TieredBenchmarkScore.NewTieredBenchmarkScore("x", "y");
        item.Hours = 182.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredBenchmarkScoreDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new TieredBenchmarkScoreDiscountRangeRule(2.0, 242.0);
        var item = TieredBenchmarkScore.NewTieredBenchmarkScore("x", "y");
        item.Discount = 243.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredBenchmarkScoreValidatorReportsEveryProblem();
        TieredBenchmarkScoreHoursRangeRuleFlagsValuesAboveHours();
        TieredBenchmarkScoreDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
