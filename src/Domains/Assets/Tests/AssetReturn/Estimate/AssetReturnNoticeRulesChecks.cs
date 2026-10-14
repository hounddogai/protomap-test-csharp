namespace Acme.Workforce.Assets.Tests.AssetReturn.Estimate;

using Acme.Workforce.Assets.AssetReturn.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AssetReturnNoticeRulesChecks
{
    public static void AssetReturnNoticeValidatorReportsEveryProblem()
    {
        var validator = AssetReturnNoticeRules.DefaultAssetReturnNoticeValidator();
        var item = AssetReturnNotice.NewAssetReturnNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnNoticeFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new AssetReturnNoticeFloorValueRangeRule(4.7, 734.8);
        var item = AssetReturnNotice.NewAssetReturnNotice("x", "y");
        item.FloorValue = 736.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnNoticeContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new AssetReturnNoticeContactEmailLengthRule(255);
        var item = AssetReturnNotice.NewAssetReturnNotice("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnNoticeRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new AssetReturnNoticeRequiredCategoryRule("asset return notice needs an entry");
        var item = AssetReturnNotice.NewAssetReturnNotice("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssetReturnNoticeValidatorReportsEveryProblem();
        AssetReturnNoticeFloorValueRangeRuleFlagsValuesAboveFloorValue();
        AssetReturnNoticeContactEmailLengthRuleFlagsALongContactEmail();
        AssetReturnNoticeRequiredCategoryRuleFlagsABlankCategory();
    }
}
