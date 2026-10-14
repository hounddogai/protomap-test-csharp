namespace Acme.Workforce.Payrollops.Tests.Deduction.Proposal;

using Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SeasonalDeductionQuotaRulesChecks
{
    public static void SeasonalDeductionQuotaValidatorReportsEveryProblem()
    {
        var validator = SeasonalDeductionQuotaRules.DefaultSeasonalDeductionQuotaValidator();
        var item = SeasonalDeductionQuota.NewSeasonalDeductionQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalDeductionQuotaDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new SeasonalDeductionQuotaDiscountRangeRule(7.6, 2584.3);
        var item = SeasonalDeductionQuota.NewSeasonalDeductionQuota("x", "y");
        item.Discount = 2585.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalDeductionQuotaRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SeasonalDeductionQuotaRequiredLabelRule("seasonal deduction quota needs this field");
        var item = SeasonalDeductionQuota.NewSeasonalDeductionQuota("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalDeductionQuotaRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new SeasonalDeductionQuotaRateRangeRule(1.3, 271.0);
        var item = SeasonalDeductionQuota.NewSeasonalDeductionQuota("x", "y");
        item.Rate = 272.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalDeductionQuotaTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SeasonalDeductionQuotaTitleLengthRule(16);
        var item = SeasonalDeductionQuota.NewSeasonalDeductionQuota("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalDeductionQuotaLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SeasonalDeductionQuotaLabelLengthRule(32);
        var item = SeasonalDeductionQuota.NewSeasonalDeductionQuota("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalDeductionQuotaValidatorReportsEveryProblem();
        SeasonalDeductionQuotaDiscountRangeRuleFlagsValuesAboveDiscount();
        SeasonalDeductionQuotaRequiredLabelRuleFlagsABlankLabel();
        SeasonalDeductionQuotaRateRangeRuleFlagsValuesAboveRate();
        SeasonalDeductionQuotaTitleLengthRuleFlagsALongTitle();
        SeasonalDeductionQuotaLabelLengthRuleFlagsALongLabel();
    }
}
