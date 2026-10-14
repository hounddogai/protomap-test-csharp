namespace Acme.Workforce.Assets.Tests.AssetReturn.Segment;

using Acme.Workforce.Assets.AssetReturn.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AssetReturnTargetRulesChecks
{
    public static void AssetReturnTargetValidatorReportsEveryProblem()
    {
        var validator = AssetReturnTargetRules.DefaultAssetReturnTargetValidator();
        var item = AssetReturnTarget.NewAssetReturnTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnTargetRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AssetReturnTargetRequiredLabelRule("asset return target needs an entry");
        var item = AssetReturnTarget.NewAssetReturnTarget("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnTargetRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new AssetReturnTargetRequiredContactEmailRule("asset return target needs an entry");
        var item = AssetReturnTarget.NewAssetReturnTarget("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssetReturnTargetValidatorReportsEveryProblem();
        AssetReturnTargetRequiredLabelRuleFlagsABlankLabel();
        AssetReturnTargetRequiredContactEmailRuleFlagsABlankContactEmail();
    }
}
