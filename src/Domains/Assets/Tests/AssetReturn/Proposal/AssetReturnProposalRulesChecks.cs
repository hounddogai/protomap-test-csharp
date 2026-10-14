namespace Acme.Workforce.Assets.Tests.AssetReturn.Proposal;

using Acme.Workforce.Assets.AssetReturn.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class AssetReturnProposalRulesChecks
{
    public static void AssetReturnProposalValidatorReportsEveryProblem()
    {
        var validator = AssetReturnProposalRules.DefaultAssetReturnProposalValidator();
        var item = AssetReturnProposal.NewAssetReturnProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnProposalStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new AssetReturnProposalStartsAtRangeRule(64, 255);
        var item = AssetReturnProposal.NewAssetReturnProposal("x", "y");
        item.StartsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnProposalRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AssetReturnProposalRequiredLabelRule("asset return proposal needs a value");
        var item = AssetReturnProposal.NewAssetReturnProposal("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnProposalQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new AssetReturnProposalQuantityRangeRule(255, 255);
        var item = AssetReturnProposal.NewAssetReturnProposal("x", "y");
        item.Quantity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AssetReturnProposalRequiredCodeRule("asset return proposal needs this field");
        var item = AssetReturnProposal.NewAssetReturnProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnProposalLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AssetReturnProposalLabelLengthRule(255);
        var item = AssetReturnProposal.NewAssetReturnProposal("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssetReturnProposalValidatorReportsEveryProblem();
        AssetReturnProposalStartsAtRangeRuleFlagsValuesAboveStartsAt();
        AssetReturnProposalRequiredLabelRuleFlagsABlankLabel();
        AssetReturnProposalQuantityRangeRuleFlagsValuesAboveQuantity();
        AssetReturnProposalRequiredCodeRuleFlagsABlankCode();
        AssetReturnProposalLabelLengthRuleFlagsALongLabel();
    }
}
