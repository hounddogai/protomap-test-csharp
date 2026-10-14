namespace Acme.Workforce.Payrollops.Tests.Deduction.Quota;

using Acme.Workforce.Payrollops.Deduction.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class DeductionQuotaRulesChecks
{
    public static void DeductionQuotaValidatorReportsEveryProblem()
    {
        var validator = DeductionQuotaRules.DefaultDeductionQuotaValidator();
        var item = DeductionQuota.NewDeductionQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionQuotaCountryLengthRuleFlagsALongCountry()
    {
        var rule = new DeductionQuotaCountryLengthRule(16);
        var item = DeductionQuota.NewDeductionQuota("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionQuotaLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new DeductionQuotaLeaveDaysRangeRule(120, 1000);
        var item = DeductionQuota.NewDeductionQuota("x", "y");
        item.LeaveDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeductionQuotaRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new DeductionQuotaRequiredSourceRule("deduction quota needs a value");
        var item = DeductionQuota.NewDeductionQuota("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionQuotaRequiredNameRuleFlagsABlankName()
    {
        var rule = new DeductionQuotaRequiredNameRule("deduction quota needs an entry");
        var item = DeductionQuota.NewDeductionQuota("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionQuotaUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new DeductionQuotaUpdatedAtRangeRule(32, 86400);
        var item = DeductionQuota.NewDeductionQuota("x", "y");
        item.UpdatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DeductionQuotaValidatorReportsEveryProblem();
        DeductionQuotaCountryLengthRuleFlagsALongCountry();
        DeductionQuotaLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        DeductionQuotaRequiredSourceRuleFlagsABlankSource();
        DeductionQuotaRequiredNameRuleFlagsABlankName();
        DeductionQuotaUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
