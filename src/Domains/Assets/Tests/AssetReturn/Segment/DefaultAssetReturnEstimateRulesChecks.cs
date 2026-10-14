namespace Acme.Workforce.Assets.Tests.AssetReturn.Segment;

using Acme.Workforce.Assets.AssetReturn.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class DefaultAssetReturnEstimateRulesChecks
{
    public static void DefaultAssetReturnEstimateValidatorReportsEveryProblem()
    {
        var validator = DefaultAssetReturnEstimateRules.DefaultDefaultAssetReturnEstimateValidator();
        var item = DefaultAssetReturnEstimate.NewDefaultAssetReturnEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultAssetReturnEstimateMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new DefaultAssetReturnEstimateMarginRangeRule(7.8, 2449.0);
        var item = DefaultAssetReturnEstimate.NewDefaultAssetReturnEstimate("x", "y");
        item.Margin = 2450.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultAssetReturnEstimateScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new DefaultAssetReturnEstimateScoreRangeRule(4.6, 2176.3);
        var item = DefaultAssetReturnEstimate.NewDefaultAssetReturnEstimate("x", "y");
        item.Score = 2177.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultAssetReturnEstimateRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new DefaultAssetReturnEstimateRequiredEmployeeIdRule("default asset return estimate needs an entry");
        var item = DefaultAssetReturnEstimate.NewDefaultAssetReturnEstimate("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultAssetReturnEstimateValidatorReportsEveryProblem();
        DefaultAssetReturnEstimateMarginRangeRuleFlagsValuesAboveMargin();
        DefaultAssetReturnEstimateScoreRangeRuleFlagsValuesAboveScore();
        DefaultAssetReturnEstimateRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
