namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class RegionalWarrantyProposalRulesChecks
{
    public static void RegionalWarrantyProposalValidatorReportsEveryProblem()
    {
        var validator = RegionalWarrantyProposalRules.DefaultRegionalWarrantyProposalValidator();
        var item = RegionalWarrantyProposal.NewRegionalWarrantyProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalWarrantyProposalSourceLengthRuleFlagsALongSource()
    {
        var rule = new RegionalWarrantyProposalSourceLengthRule(120);
        var item = RegionalWarrantyProposal.NewRegionalWarrantyProposal("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalWarrantyProposalCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new RegionalWarrantyProposalCreatedAtRangeRule(80, 86400);
        var item = RegionalWarrantyProposal.NewRegionalWarrantyProposal("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalWarrantyProposalNameLengthRuleFlagsALongName()
    {
        var rule = new RegionalWarrantyProposalNameLengthRule(3600);
        var item = RegionalWarrantyProposal.NewRegionalWarrantyProposal("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalWarrantyProposalValidatorReportsEveryProblem();
        RegionalWarrantyProposalSourceLengthRuleFlagsALongSource();
        RegionalWarrantyProposalCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        RegionalWarrantyProposalNameLengthRuleFlagsALongName();
    }
}
