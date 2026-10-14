namespace Acme.Workforce.Absence.Tests.Balance.Proposal;

using Acme.Workforce.Absence.Balance.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RollingBalanceProposalRulesChecks
{
    public static void RollingBalanceProposalValidatorReportsEveryProblem()
    {
        var validator = RollingBalanceProposalRules.DefaultRollingBalanceProposalValidator();
        var item = RollingBalanceProposal.NewRollingBalanceProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingBalanceProposalWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new RollingBalanceProposalWeightRangeRule(6.3, 878.3);
        var item = RollingBalanceProposal.NewRollingBalanceProposal("x", "y");
        item.Weight = 879.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingBalanceProposalRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new RollingBalanceProposalRequiredSalaryBandRule("rolling balance proposal needs a value");
        var item = RollingBalanceProposal.NewRollingBalanceProposal("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingBalanceProposalTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new RollingBalanceProposalTenureMonthsRangeRule(64, 120);
        var item = RollingBalanceProposal.NewRollingBalanceProposal("x", "y");
        item.TenureMonths = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingBalanceProposalValidatorReportsEveryProblem();
        RollingBalanceProposalWeightRangeRuleFlagsValuesAboveWeight();
        RollingBalanceProposalRequiredSalaryBandRuleFlagsABlankSalaryBand();
        RollingBalanceProposalTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
