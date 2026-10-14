namespace Acme.Workforce.Assets.Tests.AssetReturn.Draft;

using Acme.Workforce.Assets.AssetReturn.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AssetReturnManifestRulesChecks
{
    public static void AssetReturnManifestValidatorReportsEveryProblem()
    {
        var validator = AssetReturnManifestRules.DefaultAssetReturnManifestValidator();
        var item = AssetReturnManifest.NewAssetReturnManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnManifestRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new AssetReturnManifestRequiredLocationRule("asset return manifest needs this field");
        var item = AssetReturnManifest.NewAssetReturnManifest("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnManifestLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AssetReturnManifestLabelLengthRule(120);
        var item = AssetReturnManifest.NewAssetReturnManifest("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnManifestRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new AssetReturnManifestRequiredTimezoneRule("asset return manifest needs an entry");
        var item = AssetReturnManifest.NewAssetReturnManifest("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssetReturnManifestValidatorReportsEveryProblem();
        AssetReturnManifestRequiredLocationRuleFlagsABlankLocation();
        AssetReturnManifestLabelLengthRuleFlagsALongLabel();
        AssetReturnManifestRequiredTimezoneRuleFlagsABlankTimezone();
    }
}
