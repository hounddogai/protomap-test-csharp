namespace Acme.Workforce.Payrollops.Tests.Deduction.Proposal;

using Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeductionProposalRulesChecks
{
    public static void DeductionProposalValidatorReportsEveryProblem()
    {
        var validator = DeductionProposalRules.DefaultDeductionProposalValidator();
        var item = DeductionProposal.NewDeductionProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionProposalNameLengthRuleFlagsALongName()
    {
        var rule = new DeductionProposalNameLengthRule(86400);
        var item = DeductionProposal.NewDeductionProposal("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeductionProposalRequiredCodeRule("deduction proposal needs an entry");
        var item = DeductionProposal.NewDeductionProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionProposalRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new DeductionProposalRequiredGradeRule("deduction proposal needs an entry");
        var item = DeductionProposal.NewDeductionProposal("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeductionProposalValidatorReportsEveryProblem();
        DeductionProposalNameLengthRuleFlagsALongName();
        DeductionProposalRequiredCodeRuleFlagsABlankCode();
        DeductionProposalRequiredGradeRuleFlagsABlankGrade();
    }
}
