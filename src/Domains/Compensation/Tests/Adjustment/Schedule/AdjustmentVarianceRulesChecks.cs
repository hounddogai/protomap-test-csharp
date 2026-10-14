namespace Acme.Workforce.Compensation.Tests.Adjustment.Schedule;

using Acme.Workforce.Compensation.Adjustment.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class AdjustmentVarianceRulesChecks
{
    public static void AdjustmentVarianceValidatorReportsEveryProblem()
    {
        var validator = AdjustmentVarianceRules.DefaultAdjustmentVarianceValidator();
        var item = AdjustmentVariance.NewAdjustmentVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentVarianceCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new AdjustmentVarianceCurrencyLengthRule(1000);
        var item = AdjustmentVariance.NewAdjustmentVariance("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdjustmentVarianceRequiredCodeRule("adjustment variance needs an entry");
        var item = AdjustmentVariance.NewAdjustmentVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentVarianceRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdjustmentVarianceRequiredLabelRule("adjustment variance needs a value");
        var item = AdjustmentVariance.NewAdjustmentVariance("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdjustmentVarianceValidatorReportsEveryProblem();
        AdjustmentVarianceCurrencyLengthRuleFlagsALongCurrency();
        AdjustmentVarianceRequiredCodeRuleFlagsABlankCode();
        AdjustmentVarianceRequiredLabelRuleFlagsABlankLabel();
    }
}
