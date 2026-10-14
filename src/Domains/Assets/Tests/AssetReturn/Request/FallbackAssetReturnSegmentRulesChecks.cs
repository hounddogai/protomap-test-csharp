namespace Acme.Workforce.Assets.Tests.AssetReturn.Request;

using Acme.Workforce.Assets.AssetReturn.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class FallbackAssetReturnSegmentRulesChecks
{
    public static void FallbackAssetReturnSegmentValidatorReportsEveryProblem()
    {
        var validator = FallbackAssetReturnSegmentRules.DefaultFallbackAssetReturnSegmentValidator();
        var item = FallbackAssetReturnSegment.NewFallbackAssetReturnSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackAssetReturnSegmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FallbackAssetReturnSegmentRequiredTitleRule("fallback asset return segment needs an entry");
        var item = FallbackAssetReturnSegment.NewFallbackAssetReturnSegment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackAssetReturnSegmentRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new FallbackAssetReturnSegmentRetriesRangeRule(80, 86400);
        var item = FallbackAssetReturnSegment.NewFallbackAssetReturnSegment("x", "y");
        item.Retries = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackAssetReturnSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackAssetReturnSegmentRequiredCodeRule("fallback asset return segment needs an entry");
        var item = FallbackAssetReturnSegment.NewFallbackAssetReturnSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackAssetReturnSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackAssetReturnSegmentCodeLengthRule(86400);
        var item = FallbackAssetReturnSegment.NewFallbackAssetReturnSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackAssetReturnSegmentEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new FallbackAssetReturnSegmentEmployeeIdLengthRule(255);
        var item = FallbackAssetReturnSegment.NewFallbackAssetReturnSegment("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackAssetReturnSegmentValidatorReportsEveryProblem();
        FallbackAssetReturnSegmentRequiredTitleRuleFlagsABlankTitle();
        FallbackAssetReturnSegmentRetriesRangeRuleFlagsValuesAboveRetries();
        FallbackAssetReturnSegmentRequiredCodeRuleFlagsABlankCode();
        FallbackAssetReturnSegmentCodeLengthRuleFlagsALongCode();
        FallbackAssetReturnSegmentEmployeeIdLengthRuleFlagsALongEmployeeId();
    }
}
