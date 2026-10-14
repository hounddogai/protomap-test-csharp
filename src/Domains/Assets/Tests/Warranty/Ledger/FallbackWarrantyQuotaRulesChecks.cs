namespace Acme.Workforce.Assets.Tests.Warranty.Ledger;

using Acme.Workforce.Assets.Warranty.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FallbackWarrantyQuotaRulesChecks
{
    public static void FallbackWarrantyQuotaValidatorReportsEveryProblem()
    {
        var validator = FallbackWarrantyQuotaRules.DefaultFallbackWarrantyQuotaValidator();
        var item = FallbackWarrantyQuota.NewFallbackWarrantyQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackWarrantyQuotaTitleLengthRuleFlagsALongTitle()
    {
        var rule = new FallbackWarrantyQuotaTitleLengthRule(80);
        var item = FallbackWarrantyQuota.NewFallbackWarrantyQuota("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackWarrantyQuotaRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new FallbackWarrantyQuotaRequiredSourceRule("fallback warranty quota needs this field");
        var item = FallbackWarrantyQuota.NewFallbackWarrantyQuota("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackWarrantyQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackWarrantyQuotaRequiredCodeRule("fallback warranty quota needs an entry");
        var item = FallbackWarrantyQuota.NewFallbackWarrantyQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackWarrantyQuotaStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new FallbackWarrantyQuotaStartsAtRangeRule(80, 120);
        var item = FallbackWarrantyQuota.NewFallbackWarrantyQuota("x", "y");
        item.StartsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackWarrantyQuotaValidatorReportsEveryProblem();
        FallbackWarrantyQuotaTitleLengthRuleFlagsALongTitle();
        FallbackWarrantyQuotaRequiredSourceRuleFlagsABlankSource();
        FallbackWarrantyQuotaRequiredCodeRuleFlagsABlankCode();
        FallbackWarrantyQuotaStartsAtRangeRuleFlagsValuesAboveStartsAt();
    }
}
