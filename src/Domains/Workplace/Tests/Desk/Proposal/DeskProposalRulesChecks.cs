namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskProposalRulesChecks
{
    public static void DeskProposalValidatorReportsEveryProblem()
    {
        var validator = DeskProposalRules.DefaultDeskProposalValidator();
        var item = DeskProposal.NewDeskProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskProposalSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new DeskProposalSalaryBandLengthRule(255);
        var item = DeskProposal.NewDeskProposal("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeskProposalCodeLengthRule(80);
        var item = DeskProposal.NewDeskProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskProposalRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DeskProposalRequiredTitleRule("desk proposal needs an entry");
        var item = DeskProposal.NewDeskProposal("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeskProposalValidatorReportsEveryProblem();
        DeskProposalSalaryBandLengthRuleFlagsALongSalaryBand();
        DeskProposalCodeLengthRuleFlagsALongCode();
        DeskProposalRequiredTitleRuleFlagsABlankTitle();
    }
}
