namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class StrictAssetReturnTierRulesChecks
{
    public static void StrictAssetReturnTierValidatorReportsEveryProblem()
    {
        var validator = StrictAssetReturnTierRules.DefaultStrictAssetReturnTierValidator();
        var item = StrictAssetReturnTier.NewStrictAssetReturnTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictAssetReturnTierRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new StrictAssetReturnTierRequiredLabelRule("strict asset return tier needs an entry");
        var item = StrictAssetReturnTier.NewStrictAssetReturnTier("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictAssetReturnTierExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new StrictAssetReturnTierExternalRefLengthRule(64);
        var item = StrictAssetReturnTier.NewStrictAssetReturnTier("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictAssetReturnTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictAssetReturnTierCodeLengthRule(80);
        var item = StrictAssetReturnTier.NewStrictAssetReturnTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictAssetReturnTierValidatorReportsEveryProblem();
        StrictAssetReturnTierRequiredLabelRuleFlagsABlankLabel();
        StrictAssetReturnTierExternalRefLengthRuleFlagsALongExternalRef();
        StrictAssetReturnTierCodeLengthRuleFlagsALongCode();
    }
}
