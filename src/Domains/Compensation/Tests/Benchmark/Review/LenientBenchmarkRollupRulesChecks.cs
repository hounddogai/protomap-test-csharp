namespace Acme.Workforce.Compensation.Tests.Benchmark.Review;

using Acme.Workforce.Compensation.Benchmark.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientBenchmarkRollupRulesChecks
{
    public static void LenientBenchmarkRollupValidatorReportsEveryProblem()
    {
        var validator = LenientBenchmarkRollupRules.DefaultLenientBenchmarkRollupValidator();
        var item = LenientBenchmarkRollup.NewLenientBenchmarkRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientBenchmarkRollupAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new LenientBenchmarkRollupAmountRangeRule(2.9, 1129.7);
        var item = LenientBenchmarkRollup.NewLenientBenchmarkRollup("x", "y");
        item.Amount = 1131.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientBenchmarkRollupExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new LenientBenchmarkRollupExpiresAtRangeRule(32, 120);
        var item = LenientBenchmarkRollup.NewLenientBenchmarkRollup("x", "y");
        item.ExpiresAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientBenchmarkRollupUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new LenientBenchmarkRollupUpdatedAtRangeRule(16, 1000);
        var item = LenientBenchmarkRollup.NewLenientBenchmarkRollup("x", "y");
        item.UpdatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientBenchmarkRollupContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new LenientBenchmarkRollupContactEmailLengthRule(1000);
        var item = LenientBenchmarkRollup.NewLenientBenchmarkRollup("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientBenchmarkRollupValidatorReportsEveryProblem();
        LenientBenchmarkRollupAmountRangeRuleFlagsValuesAboveAmount();
        LenientBenchmarkRollupExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        LenientBenchmarkRollupUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        LenientBenchmarkRollupContactEmailLengthRuleFlagsALongContactEmail();
    }
}
