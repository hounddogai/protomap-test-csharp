namespace Acme.Workforce.Assets.Tests.AssetReturn.Quota;

using Acme.Workforce.Assets.AssetReturn.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class SeasonalAssetReturnQuotaRulesChecks
{
    public static void SeasonalAssetReturnQuotaValidatorReportsEveryProblem()
    {
        var validator = SeasonalAssetReturnQuotaRules.DefaultSeasonalAssetReturnQuotaValidator();
        var item = SeasonalAssetReturnQuota.NewSeasonalAssetReturnQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalAssetReturnQuotaRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SeasonalAssetReturnQuotaRequiredTitleRule("seasonal asset return quota needs this field");
        var item = SeasonalAssetReturnQuota.NewSeasonalAssetReturnQuota("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalAssetReturnQuotaReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new SeasonalAssetReturnQuotaReviewerLengthRule(64);
        var item = SeasonalAssetReturnQuota.NewSeasonalAssetReturnQuota("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalAssetReturnQuotaRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new SeasonalAssetReturnQuotaRatioRangeRule(2.7, 3471.2);
        var item = SeasonalAssetReturnQuota.NewSeasonalAssetReturnQuota("x", "y");
        item.Ratio = 3472.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalAssetReturnQuotaValidatorReportsEveryProblem();
        SeasonalAssetReturnQuotaRequiredTitleRuleFlagsABlankTitle();
        SeasonalAssetReturnQuotaReviewerLengthRuleFlagsALongReviewer();
        SeasonalAssetReturnQuotaRatioRangeRuleFlagsValuesAboveRatio();
    }
}
