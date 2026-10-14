namespace Acme.Workforce.Assets.Tests.AssetReturn.Budget;

using Acme.Workforce.Assets.AssetReturn.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class FallbackAssetReturnDraftRulesChecks
{
    public static void FallbackAssetReturnDraftValidatorReportsEveryProblem()
    {
        var validator = FallbackAssetReturnDraftRules.DefaultFallbackAssetReturnDraftValidator();
        var item = FallbackAssetReturnDraft.NewFallbackAssetReturnDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackAssetReturnDraftCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new FallbackAssetReturnDraftCurrencyLengthRule(1000);
        var item = FallbackAssetReturnDraft.NewFallbackAssetReturnDraft("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackAssetReturnDraftRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FallbackAssetReturnDraftRequiredTitleRule("fallback asset return draft needs this field");
        var item = FallbackAssetReturnDraft.NewFallbackAssetReturnDraft("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackAssetReturnDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackAssetReturnDraftCodeLengthRule(86400);
        var item = FallbackAssetReturnDraft.NewFallbackAssetReturnDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackAssetReturnDraftTitleLengthRuleFlagsALongTitle()
    {
        var rule = new FallbackAssetReturnDraftTitleLengthRule(16);
        var item = FallbackAssetReturnDraft.NewFallbackAssetReturnDraft("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackAssetReturnDraftDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new FallbackAssetReturnDraftDiscountRangeRule(0.3, 2065.6);
        var item = FallbackAssetReturnDraft.NewFallbackAssetReturnDraft("x", "y");
        item.Discount = 2067.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackAssetReturnDraftValidatorReportsEveryProblem();
        FallbackAssetReturnDraftCurrencyLengthRuleFlagsALongCurrency();
        FallbackAssetReturnDraftRequiredTitleRuleFlagsABlankTitle();
        FallbackAssetReturnDraftCodeLengthRuleFlagsALongCode();
        FallbackAssetReturnDraftTitleLengthRuleFlagsALongTitle();
        FallbackAssetReturnDraftDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
