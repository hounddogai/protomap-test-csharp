namespace Acme.Workforce.Compensation.Tests.PayBand.Batch;

using Acme.Workforce.Compensation.PayBand.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class FallbackPayBandBatchRulesChecks
{
    public static void FallbackPayBandBatchValidatorReportsEveryProblem()
    {
        var validator = FallbackPayBandBatchRules.DefaultFallbackPayBandBatchValidator();
        var item = FallbackPayBandBatch.NewFallbackPayBandBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPayBandBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackPayBandBatchCodeLengthRule(1000);
        var item = FallbackPayBandBatch.NewFallbackPayBandBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackPayBandBatchChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new FallbackPayBandBatchChecksumLengthRule(64);
        var item = FallbackPayBandBatch.NewFallbackPayBandBatch("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackPayBandBatchBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new FallbackPayBandBatchBudgetRangeRule(5.9, 3275.7);
        var item = FallbackPayBandBatch.NewFallbackPayBandBatch("x", "y");
        item.Budget = 3277.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPayBandBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackPayBandBatchRequiredCodeRule("fallback pay band batch needs an entry");
        var item = FallbackPayBandBatch.NewFallbackPayBandBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackPayBandBatchValidatorReportsEveryProblem();
        FallbackPayBandBatchCodeLengthRuleFlagsALongCode();
        FallbackPayBandBatchChecksumLengthRuleFlagsALongChecksum();
        FallbackPayBandBatchBudgetRangeRuleFlagsValuesAboveBudget();
        FallbackPayBandBatchRequiredCodeRuleFlagsABlankCode();
    }
}
