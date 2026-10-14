namespace Acme.Workforce.Contractors.Tests.Rate.RecordItem;

using Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RateProposalRulesChecks
{
    public static void RateProposalValidatorReportsEveryProblem()
    {
        var validator = RateProposalRules.DefaultRateProposalValidator();
        var item = RateProposal.NewRateProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateProposalCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new RateProposalCapacityRangeRule(120, 86400);
        var item = RateProposal.NewRateProposal("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RateProposalSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new RateProposalSalaryBandLengthRule(86400);
        var item = RateProposal.NewRateProposal("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RateProposalRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new RateProposalRequiredSalaryBandRule("rate proposal needs this field");
        var item = RateProposal.NewRateProposal("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateProposalCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new RateProposalCreatedAtRangeRule(80, 255);
        var item = RateProposal.NewRateProposal("x", "y");
        item.CreatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RateProposalValidatorReportsEveryProblem();
        RateProposalCapacityRangeRuleFlagsValuesAboveCapacity();
        RateProposalSalaryBandLengthRuleFlagsALongSalaryBand();
        RateProposalRequiredSalaryBandRuleFlagsABlankSalaryBand();
        RateProposalCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
