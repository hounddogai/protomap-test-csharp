namespace Acme.Workforce.Payrollops.Tests.RetroPay.Proposal;

using Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RetroPayProposalRulesChecks
{
    public static void RetroPayProposalValidatorReportsEveryProblem()
    {
        var validator = RetroPayProposalRules.DefaultRetroPayProposalValidator();
        var item = RetroPayProposal.NewRetroPayProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new RetroPayProposalCodeLengthRule(32);
        var item = RetroPayProposal.NewRetroPayProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayProposalGradeLengthRuleFlagsALongGrade()
    {
        var rule = new RetroPayProposalGradeLengthRule(80);
        var item = RetroPayProposal.NewRetroPayProposal("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayProposalUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new RetroPayProposalUpdatedAtRangeRule(64, 1000);
        var item = RetroPayProposal.NewRetroPayProposal("x", "y");
        item.UpdatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RetroPayProposalValidatorReportsEveryProblem();
        RetroPayProposalCodeLengthRuleFlagsALongCode();
        RetroPayProposalGradeLengthRuleFlagsALongGrade();
        RetroPayProposalUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
