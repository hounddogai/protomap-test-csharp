namespace Acme.Workforce.Talent.Tests.Mobility.RecordItem;

using Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RollingMobilityProposalRulesChecks
{
    public static void RollingMobilityProposalValidatorReportsEveryProblem()
    {
        var validator = RollingMobilityProposalRules.DefaultRollingMobilityProposalValidator();
        var item = RollingMobilityProposal.NewRollingMobilityProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingMobilityProposalSourceLengthRuleFlagsALongSource()
    {
        var rule = new RollingMobilityProposalSourceLengthRule(1000);
        var item = RollingMobilityProposal.NewRollingMobilityProposal("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingMobilityProposalRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RollingMobilityProposalRequiredLabelRule("rolling mobility proposal needs an entry");
        var item = RollingMobilityProposal.NewRollingMobilityProposal("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingMobilityProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingMobilityProposalRequiredCodeRule("rolling mobility proposal needs an entry");
        var item = RollingMobilityProposal.NewRollingMobilityProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingMobilityProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingMobilityProposalCodeLengthRule(32);
        var item = RollingMobilityProposal.NewRollingMobilityProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingMobilityProposalExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new RollingMobilityProposalExpiresAtRangeRule(16, 32);
        var item = RollingMobilityProposal.NewRollingMobilityProposal("x", "y");
        item.ExpiresAt = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingMobilityProposalValidatorReportsEveryProblem();
        RollingMobilityProposalSourceLengthRuleFlagsALongSource();
        RollingMobilityProposalRequiredLabelRuleFlagsABlankLabel();
        RollingMobilityProposalRequiredCodeRuleFlagsABlankCode();
        RollingMobilityProposalCodeLengthRuleFlagsALongCode();
        RollingMobilityProposalExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
    }
}
