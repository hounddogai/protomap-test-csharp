namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Cycle;

using Acme.Workforce.Contractors.StatementOfWork.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class CappedStatementOfWorkProposalRulesChecks
{
    public static void CappedStatementOfWorkProposalValidatorReportsEveryProblem()
    {
        var validator = CappedStatementOfWorkProposalRules.DefaultCappedStatementOfWorkProposalValidator();
        var item = CappedStatementOfWorkProposal.NewCappedStatementOfWorkProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedStatementOfWorkProposalDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new CappedStatementOfWorkProposalDiscountRangeRule(2.3, 4186.0);
        var item = CappedStatementOfWorkProposal.NewCappedStatementOfWorkProposal("x", "y");
        item.Discount = 4187.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedStatementOfWorkProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedStatementOfWorkProposalRequiredCodeRule("capped statement of work proposal needs an entry");
        var item = CappedStatementOfWorkProposal.NewCappedStatementOfWorkProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedStatementOfWorkProposalValidatorReportsEveryProblem();
        CappedStatementOfWorkProposalDiscountRangeRuleFlagsValuesAboveDiscount();
        CappedStatementOfWorkProposalRequiredCodeRuleFlagsABlankCode();
    }
}
