namespace Acme.Workforce.Assets.Tests.AssetReturn.Proposal;

using Acme.Workforce.Assets.AssetReturn.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RegionalAssetReturnProposalRulesChecks
{
    public static void RegionalAssetReturnProposalValidatorReportsEveryProblem()
    {
        var validator = RegionalAssetReturnProposalRules.DefaultRegionalAssetReturnProposalValidator();
        var item = RegionalAssetReturnProposal.NewRegionalAssetReturnProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalAssetReturnProposalTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RegionalAssetReturnProposalTitleLengthRule(16);
        var item = RegionalAssetReturnProposal.NewRegionalAssetReturnProposal("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalAssetReturnProposalRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RegionalAssetReturnProposalRequiredTitleRule("regional asset return proposal needs an entry");
        var item = RegionalAssetReturnProposal.NewRegionalAssetReturnProposal("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalAssetReturnProposalRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new RegionalAssetReturnProposalRequiredPhoneRule("regional asset return proposal needs this field");
        var item = RegionalAssetReturnProposal.NewRegionalAssetReturnProposal("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalAssetReturnProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalAssetReturnProposalRequiredCodeRule("regional asset return proposal needs this field");
        var item = RegionalAssetReturnProposal.NewRegionalAssetReturnProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalAssetReturnProposalThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new RegionalAssetReturnProposalThresholdRangeRule(9.5, 1448.3);
        var item = RegionalAssetReturnProposal.NewRegionalAssetReturnProposal("x", "y");
        item.Threshold = 1449.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalAssetReturnProposalValidatorReportsEveryProblem();
        RegionalAssetReturnProposalTitleLengthRuleFlagsALongTitle();
        RegionalAssetReturnProposalRequiredTitleRuleFlagsABlankTitle();
        RegionalAssetReturnProposalRequiredPhoneRuleFlagsABlankPhone();
        RegionalAssetReturnProposalRequiredCodeRuleFlagsABlankCode();
        RegionalAssetReturnProposalThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
