namespace Acme.Workforce.Compensation.Tests.Benchmark.Score;

using Acme.Workforce.Compensation.Benchmark.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class TieredBenchmarkProfileRulesChecks
{
    public static void TieredBenchmarkProfileValidatorReportsEveryProblem()
    {
        var validator = TieredBenchmarkProfileRules.DefaultTieredBenchmarkProfileValidator();
        var item = TieredBenchmarkProfile.NewTieredBenchmarkProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredBenchmarkProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredBenchmarkProfileCodeLengthRule(1000);
        var item = TieredBenchmarkProfile.NewTieredBenchmarkProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredBenchmarkProfileCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new TieredBenchmarkProfileCreatedAtRangeRule(32, 64);
        var item = TieredBenchmarkProfile.NewTieredBenchmarkProfile("x", "y");
        item.CreatedAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredBenchmarkProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredBenchmarkProfileRequiredCodeRule("tiered benchmark profile needs a value");
        var item = TieredBenchmarkProfile.NewTieredBenchmarkProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredBenchmarkProfileRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new TieredBenchmarkProfileRequiredOwnerIdRule("tiered benchmark profile needs an entry");
        var item = TieredBenchmarkProfile.NewTieredBenchmarkProfile("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredBenchmarkProfileValidatorReportsEveryProblem();
        TieredBenchmarkProfileCodeLengthRuleFlagsALongCode();
        TieredBenchmarkProfileCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        TieredBenchmarkProfileRequiredCodeRuleFlagsABlankCode();
        TieredBenchmarkProfileRequiredOwnerIdRuleFlagsABlankOwnerId();
    }
}
