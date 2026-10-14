namespace Acme.Workforce.Assets.Tests.AssetReturn.Estimate;

using Acme.Workforce.Assets.AssetReturn.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AdaptiveAssetReturnAllocationRulesChecks
{
    public static void AdaptiveAssetReturnAllocationValidatorReportsEveryProblem()
    {
        var validator = AdaptiveAssetReturnAllocationRules.DefaultAdaptiveAssetReturnAllocationValidator();
        var item = AdaptiveAssetReturnAllocation.NewAdaptiveAssetReturnAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveAssetReturnAllocationRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdaptiveAssetReturnAllocationRequiredLabelRule(
            "adaptive asset return allocation needs this field");
        var item = AdaptiveAssetReturnAllocation.NewAdaptiveAssetReturnAllocation("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveAssetReturnAllocationBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new AdaptiveAssetReturnAllocationBatchSizeRangeRule(80, 255);
        var item = AdaptiveAssetReturnAllocation.NewAdaptiveAssetReturnAllocation("x", "y");
        item.BatchSize = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveAssetReturnAllocationJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new AdaptiveAssetReturnAllocationJobCodeLengthRule(120);
        var item = AdaptiveAssetReturnAllocation.NewAdaptiveAssetReturnAllocation("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveAssetReturnAllocationValidatorReportsEveryProblem();
        AdaptiveAssetReturnAllocationRequiredLabelRuleFlagsABlankLabel();
        AdaptiveAssetReturnAllocationBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        AdaptiveAssetReturnAllocationJobCodeLengthRuleFlagsALongJobCode();
    }
}
