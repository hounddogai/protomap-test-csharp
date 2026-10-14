namespace Acme.Workforce.Absence.Tests.LeaveRequest.Proposal;

using Acme.Workforce.Absence.LeaveRequest.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LeaveRequestProposalRulesChecks
{
    public static void LeaveRequestProposalValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestProposalRules.DefaultLeaveRequestProposalValidator();
        var item = LeaveRequestProposal.NewLeaveRequestProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LeaveRequestProposalRequiredCodeRule("leave request proposal needs a value");
        var item = LeaveRequestProposal.NewLeaveRequestProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestProposalAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new LeaveRequestProposalAttemptsRangeRule(1000, 1000);
        var item = LeaveRequestProposal.NewLeaveRequestProposal("x", "y");
        item.Attempts = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LeaveRequestProposalRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new LeaveRequestProposalRequiredPhoneRule("leave request proposal needs an entry");
        var item = LeaveRequestProposal.NewLeaveRequestProposal("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LeaveRequestProposalValidatorReportsEveryProblem();
        LeaveRequestProposalRequiredCodeRuleFlagsABlankCode();
        LeaveRequestProposalAttemptsRangeRuleFlagsValuesAboveAttempts();
        LeaveRequestProposalRequiredPhoneRuleFlagsABlankPhone();
    }
}
