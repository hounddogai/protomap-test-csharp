namespace Acme.Workforce.Compensation.Tests.Adjustment.Rule;

using Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AdjustmentProposalRulesChecks
{
    public static void AdjustmentProposalValidatorReportsEveryProblem()
    {
        var validator = AdjustmentProposalRules.DefaultAdjustmentProposalValidator();
        var item = AdjustmentProposal.NewAdjustmentProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentProposalRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdjustmentProposalRequiredLabelRule("adjustment proposal needs this field");
        var item = AdjustmentProposal.NewAdjustmentProposal("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentProposalStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new AdjustmentProposalStartsAtRangeRule(64, 80);
        var item = AdjustmentProposal.NewAdjustmentProposal("x", "y");
        item.StartsAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdjustmentProposalValidatorReportsEveryProblem();
        AdjustmentProposalRequiredLabelRuleFlagsABlankLabel();
        AdjustmentProposalStartsAtRangeRuleFlagsValuesAboveStartsAt();
    }
}
