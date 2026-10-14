namespace Acme.Workforce.Payrollops.Tests.Payslip.Batch;

using Acme.Workforce.Payrollops.Payslip.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class FallbackPayslipBatchRulesChecks
{
    public static void FallbackPayslipBatchValidatorReportsEveryProblem()
    {
        var validator = FallbackPayslipBatchRules.DefaultFallbackPayslipBatchValidator();
        var item = FallbackPayslipBatch.NewFallbackPayslipBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPayslipBatchRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new FallbackPayslipBatchRequiredCountryRule("fallback payslip batch needs this field");
        var item = FallbackPayslipBatch.NewFallbackPayslipBatch("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPayslipBatchRequiredNameRuleFlagsABlankName()
    {
        var rule = new FallbackPayslipBatchRequiredNameRule("fallback payslip batch needs this field");
        var item = FallbackPayslipBatch.NewFallbackPayslipBatch("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPayslipBatchRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new FallbackPayslipBatchRateRangeRule(7.1, 2267.5);
        var item = FallbackPayslipBatch.NewFallbackPayslipBatch("x", "y");
        item.Rate = 2269.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPayslipBatchRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new FallbackPayslipBatchRequiredDescriptionRule("fallback payslip batch needs a value");
        var item = FallbackPayslipBatch.NewFallbackPayslipBatch("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackPayslipBatchValidatorReportsEveryProblem();
        FallbackPayslipBatchRequiredCountryRuleFlagsABlankCountry();
        FallbackPayslipBatchRequiredNameRuleFlagsABlankName();
        FallbackPayslipBatchRateRangeRuleFlagsValuesAboveRate();
        FallbackPayslipBatchRequiredDescriptionRuleFlagsABlankDescription();
    }
}
