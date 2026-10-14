namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAllocation;

using Acme.Workforce.Payrollops.PayRun.RunAllocation;

/// <summary>Checks of run allocation code, run by the project's runner.</summary>
public static class FallbackPayRunCycleRulesChecks
{
    public static void FallbackPayRunCycleValidatorReportsEveryProblem()
    {
        var validator = FallbackPayRunCycleRules.DefaultFallbackPayRunCycleValidator();
        var item = FallbackPayRunCycle.NewFallbackPayRunCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPayRunCycleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackPayRunCycleRequiredCodeRule("fallback pay run cycle needs this field");
        var item = FallbackPayRunCycle.NewFallbackPayRunCycle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPayRunCycleCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new FallbackPayRunCycleCurrencyLengthRule(86400);
        var item = FallbackPayRunCycle.NewFallbackPayRunCycle("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackPayRunCycleCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new FallbackPayRunCycleCreatedAtRangeRule(120, 255);
        var item = FallbackPayRunCycle.NewFallbackPayRunCycle("x", "y");
        item.CreatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackPayRunCycleValidatorReportsEveryProblem();
        FallbackPayRunCycleRequiredCodeRuleFlagsABlankCode();
        FallbackPayRunCycleCurrencyLengthRuleFlagsALongCurrency();
        FallbackPayRunCycleCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
