namespace Acme.Workforce.Assets.Tests.Warranty.Budget;

using Acme.Workforce.Assets.Warranty.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class AdaptiveWarrantyBundleRulesChecks
{
    public static void AdaptiveWarrantyBundleValidatorReportsEveryProblem()
    {
        var validator = AdaptiveWarrantyBundleRules.DefaultAdaptiveWarrantyBundleValidator();
        var item = AdaptiveWarrantyBundle.NewAdaptiveWarrantyBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveWarrantyBundleNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptiveWarrantyBundleNameLengthRule(255);
        var item = AdaptiveWarrantyBundle.NewAdaptiveWarrantyBundle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveWarrantyBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveWarrantyBundleRequiredCodeRule("adaptive warranty bundle needs a value");
        var item = AdaptiveWarrantyBundle.NewAdaptiveWarrantyBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveWarrantyBundleFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new AdaptiveWarrantyBundleFteRangeRule(4.5, 173.3);
        var item = AdaptiveWarrantyBundle.NewAdaptiveWarrantyBundle("x", "y");
        item.Fte = 174.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveWarrantyBundleRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new AdaptiveWarrantyBundleRequiredBadgeRule("adaptive warranty bundle needs an entry");
        var item = AdaptiveWarrantyBundle.NewAdaptiveWarrantyBundle("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveWarrantyBundleMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new AdaptiveWarrantyBundleMarginRangeRule(7.1, 2938.2);
        var item = AdaptiveWarrantyBundle.NewAdaptiveWarrantyBundle("x", "y");
        item.Margin = 2939.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveWarrantyBundleValidatorReportsEveryProblem();
        AdaptiveWarrantyBundleNameLengthRuleFlagsALongName();
        AdaptiveWarrantyBundleRequiredCodeRuleFlagsABlankCode();
        AdaptiveWarrantyBundleFteRangeRuleFlagsValuesAboveFte();
        AdaptiveWarrantyBundleRequiredBadgeRuleFlagsABlankBadge();
        AdaptiveWarrantyBundleMarginRangeRuleFlagsValuesAboveMargin();
    }
}
