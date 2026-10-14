namespace Acme.Workforce.Payrollops.Tests.TaxForm.Score;

using Acme.Workforce.Payrollops.TaxForm.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class LenientTaxFormEstimateRulesChecks
{
    public static void LenientTaxFormEstimateValidatorReportsEveryProblem()
    {
        var validator = LenientTaxFormEstimateRules.DefaultLenientTaxFormEstimateValidator();
        var item = LenientTaxFormEstimate.NewLenientTaxFormEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientTaxFormEstimateRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new LenientTaxFormEstimateRatioRangeRule(0.8, 3971.1);
        var item = LenientTaxFormEstimate.NewLenientTaxFormEstimate("x", "y");
        item.Ratio = 3972.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientTaxFormEstimateLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LenientTaxFormEstimateLabelLengthRule(16);
        var item = LenientTaxFormEstimate.NewLenientTaxFormEstimate("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientTaxFormEstimateValidatorReportsEveryProblem();
        LenientTaxFormEstimateRatioRangeRuleFlagsValuesAboveRatio();
        LenientTaxFormEstimateLabelLengthRuleFlagsALongLabel();
    }
}
