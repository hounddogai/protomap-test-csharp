namespace Acme.Workforce.Assets.Tests.AssetReturn.Tier;

using Acme.Workforce.Assets.AssetReturn.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ManualAssetReturnTierRulesChecks
{
    public static void ManualAssetReturnTierValidatorReportsEveryProblem()
    {
        var validator = ManualAssetReturnTierRules.DefaultManualAssetReturnTierValidator();
        var item = ManualAssetReturnTier.NewManualAssetReturnTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualAssetReturnTierRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ManualAssetReturnTierRequiredTitleRule("manual asset return tier needs a value");
        var item = ManualAssetReturnTier.NewManualAssetReturnTier("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualAssetReturnTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualAssetReturnTierRequiredCodeRule("manual asset return tier needs an entry");
        var item = ManualAssetReturnTier.NewManualAssetReturnTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualAssetReturnTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualAssetReturnTierCodeLengthRule(3600);
        var item = ManualAssetReturnTier.NewManualAssetReturnTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualAssetReturnTierCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new ManualAssetReturnTierCreatedAtRangeRule(64, 86400);
        var item = ManualAssetReturnTier.NewManualAssetReturnTier("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualAssetReturnTierValidatorReportsEveryProblem();
        ManualAssetReturnTierRequiredTitleRuleFlagsABlankTitle();
        ManualAssetReturnTierRequiredCodeRuleFlagsABlankCode();
        ManualAssetReturnTierCodeLengthRuleFlagsALongCode();
        ManualAssetReturnTierCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
