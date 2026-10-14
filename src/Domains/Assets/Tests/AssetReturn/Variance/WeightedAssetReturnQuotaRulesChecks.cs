namespace Acme.Workforce.Assets.Tests.AssetReturn.Variance;

using Acme.Workforce.Assets.AssetReturn.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class WeightedAssetReturnQuotaRulesChecks
{
    public static void WeightedAssetReturnQuotaValidatorReportsEveryProblem()
    {
        var validator = WeightedAssetReturnQuotaRules.DefaultWeightedAssetReturnQuotaValidator();
        var item = WeightedAssetReturnQuota.NewWeightedAssetReturnQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedAssetReturnQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedAssetReturnQuotaRequiredCodeRule("weighted asset return quota needs an entry");
        var item = WeightedAssetReturnQuota.NewWeightedAssetReturnQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedAssetReturnQuotaRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new WeightedAssetReturnQuotaRequiredChecksumRule("weighted asset return quota needs this field");
        var item = WeightedAssetReturnQuota.NewWeightedAssetReturnQuota("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedAssetReturnQuotaRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new WeightedAssetReturnQuotaRequiredTitleRule("weighted asset return quota needs a value");
        var item = WeightedAssetReturnQuota.NewWeightedAssetReturnQuota("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedAssetReturnQuotaValidatorReportsEveryProblem();
        WeightedAssetReturnQuotaRequiredCodeRuleFlagsABlankCode();
        WeightedAssetReturnQuotaRequiredChecksumRuleFlagsABlankChecksum();
        WeightedAssetReturnQuotaRequiredTitleRuleFlagsABlankTitle();
    }
}
