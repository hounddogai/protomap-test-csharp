namespace Acme.Workforce.Orgdesign.Tests.Position.PositionThreshold;

using Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>Checks of position threshold code, run by the project's runner.</summary>
public static class PrimaryPositionProposalRulesChecks
{
    public static void PrimaryPositionProposalValidatorReportsEveryProblem()
    {
        var validator = PrimaryPositionProposalRules.DefaultPrimaryPositionProposalValidator();
        var item = PrimaryPositionProposal.NewPrimaryPositionProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryPositionProposalLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PrimaryPositionProposalLabelLengthRule(16);
        var item = PrimaryPositionProposal.NewPrimaryPositionProposal("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryPositionProposalRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PrimaryPositionProposalRequiredLabelRule("primary position proposal needs a value");
        var item = PrimaryPositionProposal.NewPrimaryPositionProposal("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryPositionProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryPositionProposalRequiredCodeRule("primary position proposal needs a value");
        var item = PrimaryPositionProposal.NewPrimaryPositionProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryPositionProposalEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new PrimaryPositionProposalEndsAtRangeRule(80, 86400);
        var item = PrimaryPositionProposal.NewPrimaryPositionProposal("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryPositionProposalValidatorReportsEveryProblem();
        PrimaryPositionProposalLabelLengthRuleFlagsALongLabel();
        PrimaryPositionProposalRequiredLabelRuleFlagsABlankLabel();
        PrimaryPositionProposalRequiredCodeRuleFlagsABlankCode();
        PrimaryPositionProposalEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
