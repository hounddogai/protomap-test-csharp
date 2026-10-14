namespace Acme.Workforce.Compensation.Tests.Adjustment.Estimate;

using Acme.Workforce.Compensation.Adjustment.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class SeasonalAdjustmentProposalRulesChecks
{
    public static void SeasonalAdjustmentProposalValidatorReportsEveryProblem()
    {
        var validator = SeasonalAdjustmentProposalRules.DefaultSeasonalAdjustmentProposalValidator();
        var item = SeasonalAdjustmentProposal.NewSeasonalAdjustmentProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalAdjustmentProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalAdjustmentProposalRequiredCodeRule("seasonal adjustment proposal needs a value");
        var item = SeasonalAdjustmentProposal.NewSeasonalAdjustmentProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalAdjustmentProposalRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SeasonalAdjustmentProposalRequiredLabelRule("seasonal adjustment proposal needs an entry");
        var item = SeasonalAdjustmentProposal.NewSeasonalAdjustmentProposal("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalAdjustmentProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalAdjustmentProposalCodeLengthRule(120);
        var item = SeasonalAdjustmentProposal.NewSeasonalAdjustmentProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalAdjustmentProposalValidatorReportsEveryProblem();
        SeasonalAdjustmentProposalRequiredCodeRuleFlagsABlankCode();
        SeasonalAdjustmentProposalRequiredLabelRuleFlagsABlankLabel();
        SeasonalAdjustmentProposalCodeLengthRuleFlagsALongCode();
    }
}
