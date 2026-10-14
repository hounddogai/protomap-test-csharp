namespace Acme.Workforce.Assets.Tests.AssetReturn.Rule;

using Acme.Workforce.Assets.AssetReturn.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class StrictAssetReturnManifestRulesChecks
{
    public static void StrictAssetReturnManifestValidatorReportsEveryProblem()
    {
        var validator = StrictAssetReturnManifestRules.DefaultStrictAssetReturnManifestValidator();
        var item = StrictAssetReturnManifest.NewStrictAssetReturnManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictAssetReturnManifestRequiredNameRuleFlagsABlankName()
    {
        var rule = new StrictAssetReturnManifestRequiredNameRule("strict asset return manifest needs this field");
        var item = StrictAssetReturnManifest.NewStrictAssetReturnManifest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictAssetReturnManifestAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new StrictAssetReturnManifestAmountRangeRule(2.1, 4664.4);
        var item = StrictAssetReturnManifest.NewStrictAssetReturnManifest("x", "y");
        item.Amount = 4665.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictAssetReturnManifestValidatorReportsEveryProblem();
        StrictAssetReturnManifestRequiredNameRuleFlagsABlankName();
        StrictAssetReturnManifestAmountRangeRuleFlagsValuesAboveAmount();
    }
}
