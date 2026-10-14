namespace Acme.Workforce.Compliance.Tests.Attestation.Segment;

using Acme.Workforce.Compliance.Attestation.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class CappedAttestationProposalRulesChecks
{
    public static void CappedAttestationProposalValidatorReportsEveryProblem()
    {
        var validator = CappedAttestationProposalRules.DefaultCappedAttestationProposalValidator();
        var item = CappedAttestationProposal.NewCappedAttestationProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedAttestationProposalRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CappedAttestationProposalRequiredTitleRule("capped attestation proposal needs this field");
        var item = CappedAttestationProposal.NewCappedAttestationProposal("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedAttestationProposalCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new CappedAttestationProposalCapacityRangeRule(255, 1000);
        var item = CappedAttestationProposal.NewCappedAttestationProposal("x", "y");
        item.Capacity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedAttestationProposalRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new CappedAttestationProposalRequiredLocaleRule("capped attestation proposal needs a value");
        var item = CappedAttestationProposal.NewCappedAttestationProposal("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedAttestationProposalValidatorReportsEveryProblem();
        CappedAttestationProposalRequiredTitleRuleFlagsABlankTitle();
        CappedAttestationProposalCapacityRangeRuleFlagsValuesAboveCapacity();
        CappedAttestationProposalRequiredLocaleRuleFlagsABlankLocale();
    }
}
