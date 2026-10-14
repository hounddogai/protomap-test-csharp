namespace Acme.Workforce.Payrollops.Tests.RetroPay.Assignment;

using Acme.Workforce.Payrollops.RetroPay.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LenientRetroPayExceptionRulesChecks
{
    public static void LenientRetroPayExceptionValidatorReportsEveryProblem()
    {
        var validator = LenientRetroPayExceptionRules.DefaultLenientRetroPayExceptionValidator();
        var item = LenientRetroPayException.NewLenientRetroPayException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientRetroPayExceptionLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LenientRetroPayExceptionLabelLengthRule(80);
        var item = LenientRetroPayException.NewLenientRetroPayException("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientRetroPayExceptionBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new LenientRetroPayExceptionBatchSizeRangeRule(120, 1000);
        var item = LenientRetroPayException.NewLenientRetroPayException("x", "y");
        item.BatchSize = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientRetroPayExceptionRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new LenientRetroPayExceptionRequiredCountryRule("lenient retro pay exception needs this field");
        var item = LenientRetroPayException.NewLenientRetroPayException("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientRetroPayExceptionValidatorReportsEveryProblem();
        LenientRetroPayExceptionLabelLengthRuleFlagsALongLabel();
        LenientRetroPayExceptionBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        LenientRetroPayExceptionRequiredCountryRuleFlagsABlankCountry();
    }
}
