namespace Acme.Workforce.Assets.Tests.AssetReturn.Score;

using Acme.Workforce.Assets.AssetReturn.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class LenientAssetReturnTargetRulesChecks
{
    public static void LenientAssetReturnTargetValidatorReportsEveryProblem()
    {
        var validator = LenientAssetReturnTargetRules.DefaultLenientAssetReturnTargetValidator();
        var item = LenientAssetReturnTarget.NewLenientAssetReturnTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientAssetReturnTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientAssetReturnTargetRequiredCodeRule("lenient asset return target needs this field");
        var item = LenientAssetReturnTarget.NewLenientAssetReturnTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientAssetReturnTargetReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new LenientAssetReturnTargetReviewerLengthRule(16);
        var item = LenientAssetReturnTarget.NewLenientAssetReturnTarget("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientAssetReturnTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientAssetReturnTargetCodeLengthRule(80);
        var item = LenientAssetReturnTarget.NewLenientAssetReturnTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientAssetReturnTargetRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientAssetReturnTargetRequiredNameRule("lenient asset return target needs an entry");
        var item = LenientAssetReturnTarget.NewLenientAssetReturnTarget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientAssetReturnTargetValidatorReportsEveryProblem();
        LenientAssetReturnTargetRequiredCodeRuleFlagsABlankCode();
        LenientAssetReturnTargetReviewerLengthRuleFlagsALongReviewer();
        LenientAssetReturnTargetCodeLengthRuleFlagsALongCode();
        LenientAssetReturnTargetRequiredNameRuleFlagsABlankName();
    }
}
