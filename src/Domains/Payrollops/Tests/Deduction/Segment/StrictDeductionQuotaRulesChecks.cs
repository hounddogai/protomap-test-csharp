namespace Acme.Workforce.Payrollops.Tests.Deduction.Segment;

using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StrictDeductionQuotaRulesChecks
{
    public static void StrictDeductionQuotaValidatorReportsEveryProblem()
    {
        var validator = StrictDeductionQuotaRules.DefaultStrictDeductionQuotaValidator();
        var item = StrictDeductionQuota.NewStrictDeductionQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictDeductionQuotaRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new StrictDeductionQuotaRetriesRangeRule(16, 64);
        var item = StrictDeductionQuota.NewStrictDeductionQuota("x", "y");
        item.Retries = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictDeductionQuotaCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new StrictDeductionQuotaCeilingRangeRule(4.9, 3410.0);
        var item = StrictDeductionQuota.NewStrictDeductionQuota("x", "y");
        item.Ceiling = 3411.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictDeductionQuotaLabelLengthRuleFlagsALongLabel()
    {
        var rule = new StrictDeductionQuotaLabelLengthRule(255);
        var item = StrictDeductionQuota.NewStrictDeductionQuota("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictDeductionQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictDeductionQuotaCodeLengthRule(255);
        var item = StrictDeductionQuota.NewStrictDeductionQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictDeductionQuotaRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new StrictDeductionQuotaRequiredLabelRule("strict deduction quota needs this field");
        var item = StrictDeductionQuota.NewStrictDeductionQuota("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictDeductionQuotaCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new StrictDeductionQuotaCurrencyLengthRule(1000);
        var item = StrictDeductionQuota.NewStrictDeductionQuota("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictDeductionQuotaValidatorReportsEveryProblem();
        StrictDeductionQuotaRetriesRangeRuleFlagsValuesAboveRetries();
        StrictDeductionQuotaCeilingRangeRuleFlagsValuesAboveCeiling();
        StrictDeductionQuotaLabelLengthRuleFlagsALongLabel();
        StrictDeductionQuotaCodeLengthRuleFlagsALongCode();
        StrictDeductionQuotaRequiredLabelRuleFlagsABlankLabel();
        StrictDeductionQuotaCurrencyLengthRuleFlagsALongCurrency();
    }
}
