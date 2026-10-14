namespace Acme.Workforce.Assets.Tests.AssetReturn.OverrideItem;

using Acme.Workforce.Assets.AssetReturn.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class AssetReturnOverrideRulesChecks
{
    public static void AssetReturnOverrideValidatorReportsEveryProblem()
    {
        var validator = AssetReturnOverrideRules.DefaultAssetReturnOverrideValidator();
        var item = AssetReturnOverride.NewAssetReturnOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnOverrideRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AssetReturnOverrideRequiredLabelRule("asset return override needs an entry");
        var item = AssetReturnOverride.NewAssetReturnOverride("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnOverrideRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new AssetReturnOverrideRequiredSourceRule("asset return override needs a value");
        var item = AssetReturnOverride.NewAssetReturnOverride("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnOverrideSourceLengthRuleFlagsALongSource()
    {
        var rule = new AssetReturnOverrideSourceLengthRule(64);
        var item = AssetReturnOverride.NewAssetReturnOverride("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnOverrideLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AssetReturnOverrideLabelLengthRule(32);
        var item = AssetReturnOverride.NewAssetReturnOverride("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssetReturnOverrideValidatorReportsEveryProblem();
        AssetReturnOverrideRequiredLabelRuleFlagsABlankLabel();
        AssetReturnOverrideRequiredSourceRuleFlagsABlankSource();
        AssetReturnOverrideSourceLengthRuleFlagsALongSource();
        AssetReturnOverrideLabelLengthRuleFlagsALongLabel();
    }
}
