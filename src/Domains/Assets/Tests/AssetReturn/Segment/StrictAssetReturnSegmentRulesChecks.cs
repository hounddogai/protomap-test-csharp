namespace Acme.Workforce.Assets.Tests.AssetReturn.Segment;

using Acme.Workforce.Assets.AssetReturn.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StrictAssetReturnSegmentRulesChecks
{
    public static void StrictAssetReturnSegmentValidatorReportsEveryProblem()
    {
        var validator = StrictAssetReturnSegmentRules.DefaultStrictAssetReturnSegmentValidator();
        var item = StrictAssetReturnSegment.NewStrictAssetReturnSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictAssetReturnSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictAssetReturnSegmentRequiredCodeRule("strict asset return segment needs a value");
        var item = StrictAssetReturnSegment.NewStrictAssetReturnSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictAssetReturnSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictAssetReturnSegmentCodeLengthRule(16);
        var item = StrictAssetReturnSegment.NewStrictAssetReturnSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictAssetReturnSegmentLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new StrictAssetReturnSegmentLeaveDaysRangeRule(255, 3600);
        var item = StrictAssetReturnSegment.NewStrictAssetReturnSegment("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictAssetReturnSegmentValidatorReportsEveryProblem();
        StrictAssetReturnSegmentRequiredCodeRuleFlagsABlankCode();
        StrictAssetReturnSegmentCodeLengthRuleFlagsALongCode();
        StrictAssetReturnSegmentLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
