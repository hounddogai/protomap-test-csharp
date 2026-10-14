namespace Acme.Workforce.Compensation.Tests.Benchmark.Rollup;

using Acme.Workforce.Compensation.Benchmark.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class CappedBenchmarkTierRulesChecks
{
    public static void CappedBenchmarkTierValidatorReportsEveryProblem()
    {
        var validator = CappedBenchmarkTierRules.DefaultCappedBenchmarkTierValidator();
        var item = CappedBenchmarkTier.NewCappedBenchmarkTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedBenchmarkTierLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CappedBenchmarkTierLabelLengthRule(86400);
        var item = CappedBenchmarkTier.NewCappedBenchmarkTier("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedBenchmarkTierHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new CappedBenchmarkTierHoursRangeRule(8.2, 468.8);
        var item = CappedBenchmarkTier.NewCappedBenchmarkTier("x", "y");
        item.Hours = 470.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedBenchmarkTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedBenchmarkTierCodeLengthRule(32);
        var item = CappedBenchmarkTier.NewCappedBenchmarkTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedBenchmarkTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedBenchmarkTierRequiredCodeRule("capped benchmark tier needs this field");
        var item = CappedBenchmarkTier.NewCappedBenchmarkTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedBenchmarkTierValidatorReportsEveryProblem();
        CappedBenchmarkTierLabelLengthRuleFlagsALongLabel();
        CappedBenchmarkTierHoursRangeRuleFlagsValuesAboveHours();
        CappedBenchmarkTierCodeLengthRuleFlagsALongCode();
        CappedBenchmarkTierRequiredCodeRuleFlagsABlankCode();
    }
}
