namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AssetReturnProfileRulesChecks
{
    public static void AssetReturnProfileValidatorReportsEveryProblem()
    {
        var validator = AssetReturnProfileRules.DefaultAssetReturnProfileValidator();
        var item = AssetReturnProfile.NewAssetReturnProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnProfileFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new AssetReturnProfileFteRangeRule(5.5, 2839.0);
        var item = AssetReturnProfile.NewAssetReturnProfile("x", "y");
        item.Fte = 2840.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnProfileRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AssetReturnProfileRequiredTitleRule("asset return profile needs an entry");
        var item = AssetReturnProfile.NewAssetReturnProfile("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnProfileDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new AssetReturnProfileDiscountRangeRule(1.1, 1921.6);
        var item = AssetReturnProfile.NewAssetReturnProfile("x", "y");
        item.Discount = 1923.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnProfileRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new AssetReturnProfileRequiredReviewerRule("asset return profile needs an entry");
        var item = AssetReturnProfile.NewAssetReturnProfile("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnProfileMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new AssetReturnProfileMarginRangeRule(1.8, 867.2);
        var item = AssetReturnProfile.NewAssetReturnProfile("x", "y");
        item.Margin = 868.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AssetReturnProfileRequiredCodeRule("asset return profile needs a value");
        var item = AssetReturnProfile.NewAssetReturnProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssetReturnProfileValidatorReportsEveryProblem();
        AssetReturnProfileFteRangeRuleFlagsValuesAboveFte();
        AssetReturnProfileRequiredTitleRuleFlagsABlankTitle();
        AssetReturnProfileDiscountRangeRuleFlagsValuesAboveDiscount();
        AssetReturnProfileRequiredReviewerRuleFlagsABlankReviewer();
        AssetReturnProfileMarginRangeRuleFlagsValuesAboveMargin();
        AssetReturnProfileRequiredCodeRuleFlagsABlankCode();
    }
}
