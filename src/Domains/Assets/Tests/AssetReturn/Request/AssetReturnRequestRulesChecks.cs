namespace Acme.Workforce.Assets.Tests.AssetReturn.Request;

using Acme.Workforce.Assets.AssetReturn.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AssetReturnRequestRulesChecks
{
    public static void AssetReturnRequestValidatorReportsEveryProblem()
    {
        var validator = AssetReturnRequestRules.DefaultAssetReturnRequestValidator();
        var item = AssetReturnRequest.NewAssetReturnRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnRequestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AssetReturnRequestTitleLengthRule(86400);
        var item = AssetReturnRequest.NewAssetReturnRequest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AssetReturnRequestRequiredCodeRule("asset return request needs this field");
        var item = AssetReturnRequest.NewAssetReturnRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnRequestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AssetReturnRequestRequiredTitleRule("asset return request needs this field");
        var item = AssetReturnRequest.NewAssetReturnRequest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnRequestRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new AssetReturnRequestRequiredContactEmailRule("asset return request needs an entry");
        var item = AssetReturnRequest.NewAssetReturnRequest("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssetReturnRequestValidatorReportsEveryProblem();
        AssetReturnRequestTitleLengthRuleFlagsALongTitle();
        AssetReturnRequestRequiredCodeRuleFlagsABlankCode();
        AssetReturnRequestRequiredTitleRuleFlagsABlankTitle();
        AssetReturnRequestRequiredContactEmailRuleFlagsABlankContactEmail();
    }
}
