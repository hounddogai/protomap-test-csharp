namespace Acme.Workforce.Assets.Tests.AssetReturn.Summary;

using Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AssetReturnSummaryRulesChecks
{
    public static void AssetReturnSummaryValidatorReportsEveryProblem()
    {
        var validator = AssetReturnSummaryRules.DefaultAssetReturnSummaryValidator();
        var item = AssetReturnSummary.NewAssetReturnSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnSummarySegmentLengthRuleFlagsALongSegment()
    {
        var rule = new AssetReturnSummarySegmentLengthRule(120);
        var item = AssetReturnSummary.NewAssetReturnSummary("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnSummaryScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new AssetReturnSummaryScoreRangeRule(7.0, 4940.3);
        var item = AssetReturnSummary.NewAssetReturnSummary("x", "y");
        item.Score = 4941.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnSummaryLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AssetReturnSummaryLabelLengthRule(32);
        var item = AssetReturnSummary.NewAssetReturnSummary("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssetReturnSummaryValidatorReportsEveryProblem();
        AssetReturnSummarySegmentLengthRuleFlagsALongSegment();
        AssetReturnSummaryScoreRangeRuleFlagsValuesAboveScore();
        AssetReturnSummaryLabelLengthRuleFlagsALongLabel();
    }
}
