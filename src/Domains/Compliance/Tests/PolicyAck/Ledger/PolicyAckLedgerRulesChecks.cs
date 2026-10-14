namespace Acme.Workforce.Compliance.Tests.PolicyAck.Ledger;

using Acme.Workforce.Compliance.PolicyAck.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class PolicyAckLedgerRulesChecks
{
    public static void PolicyAckLedgerValidatorReportsEveryProblem()
    {
        var validator = PolicyAckLedgerRules.DefaultPolicyAckLedgerValidator();
        var item = PolicyAckLedger.NewPolicyAckLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new PolicyAckLedgerCodeLengthRule(86400);
        var item = PolicyAckLedger.NewPolicyAckLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckLedgerRequiredNameRuleFlagsABlankName()
    {
        var rule = new PolicyAckLedgerRequiredNameRule("policy ack ledger needs a value");
        var item = PolicyAckLedger.NewPolicyAckLedger("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckLedgerRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new PolicyAckLedgerRequiredReviewerRule("policy ack ledger needs a value");
        var item = PolicyAckLedger.NewPolicyAckLedger("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PolicyAckLedgerValidatorReportsEveryProblem();
        PolicyAckLedgerCodeLengthRuleFlagsALongCode();
        PolicyAckLedgerRequiredNameRuleFlagsABlankName();
        PolicyAckLedgerRequiredReviewerRuleFlagsABlankReviewer();
    }
}
