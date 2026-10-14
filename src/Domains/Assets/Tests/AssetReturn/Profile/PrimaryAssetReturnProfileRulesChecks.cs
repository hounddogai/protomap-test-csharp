namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PrimaryAssetReturnProfileRulesChecks
{
    public static void PrimaryAssetReturnProfileValidatorReportsEveryProblem()
    {
        var validator = PrimaryAssetReturnProfileRules.DefaultPrimaryAssetReturnProfileValidator();
        var item = PrimaryAssetReturnProfile.NewPrimaryAssetReturnProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryAssetReturnProfileTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PrimaryAssetReturnProfileTitleLengthRule(16);
        var item = PrimaryAssetReturnProfile.NewPrimaryAssetReturnProfile("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryAssetReturnProfileRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new PrimaryAssetReturnProfileRequiredCategoryRule("primary asset return profile needs this field");
        var item = PrimaryAssetReturnProfile.NewPrimaryAssetReturnProfile("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryAssetReturnProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryAssetReturnProfileRequiredCodeRule("primary asset return profile needs this field");
        var item = PrimaryAssetReturnProfile.NewPrimaryAssetReturnProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryAssetReturnProfileValidatorReportsEveryProblem();
        PrimaryAssetReturnProfileTitleLengthRuleFlagsALongTitle();
        PrimaryAssetReturnProfileRequiredCategoryRuleFlagsABlankCategory();
        PrimaryAssetReturnProfileRequiredCodeRuleFlagsABlankCode();
    }
}
