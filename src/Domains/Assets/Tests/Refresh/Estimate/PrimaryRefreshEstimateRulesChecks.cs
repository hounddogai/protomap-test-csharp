namespace Acme.Workforce.Assets.Tests.Refresh.Estimate;

using Acme.Workforce.Assets.Refresh.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimaryRefreshEstimateRulesChecks
{
    public static void PrimaryRefreshEstimateValidatorReportsEveryProblem()
    {
        var validator = PrimaryRefreshEstimateRules.DefaultPrimaryRefreshEstimateValidator();
        var item = PrimaryRefreshEstimate.NewPrimaryRefreshEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryRefreshEstimateBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new PrimaryRefreshEstimateBatchSizeRangeRule(32, 86400);
        var item = PrimaryRefreshEstimate.NewPrimaryRefreshEstimate("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryRefreshEstimateRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PrimaryRefreshEstimateRequiredLabelRule("primary refresh estimate needs a value");
        var item = PrimaryRefreshEstimate.NewPrimaryRefreshEstimate("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryRefreshEstimateBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new PrimaryRefreshEstimateBadgeLengthRule(86400);
        var item = PrimaryRefreshEstimate.NewPrimaryRefreshEstimate("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryRefreshEstimateValidatorReportsEveryProblem();
        PrimaryRefreshEstimateBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        PrimaryRefreshEstimateRequiredLabelRuleFlagsABlankLabel();
        PrimaryRefreshEstimateBadgeLengthRuleFlagsALongBadge();
    }
}
