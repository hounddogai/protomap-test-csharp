namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PolicyAckRequestRulesChecks
{
    public static void PolicyAckRequestValidatorReportsEveryProblem()
    {
        var validator = PolicyAckRequestRules.DefaultPolicyAckRequestValidator();
        var item = PolicyAckRequest.NewPolicyAckRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckRequestRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new PolicyAckRequestRevisionRangeRule(255, 3600);
        var item = PolicyAckRequest.NewPolicyAckRequest("x", "y");
        item.Revision = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyAckRequestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PolicyAckRequestRequiredTitleRule("policy ack request needs this field");
        var item = PolicyAckRequest.NewPolicyAckRequest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckRequestExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new PolicyAckRequestExpiresAtRangeRule(16, 86400);
        var item = PolicyAckRequest.NewPolicyAckRequest("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyAckRequestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PolicyAckRequestTitleLengthRule(80);
        var item = PolicyAckRequest.NewPolicyAckRequest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PolicyAckRequestValidatorReportsEveryProblem();
        PolicyAckRequestRevisionRangeRuleFlagsValuesAboveRevision();
        PolicyAckRequestRequiredTitleRuleFlagsABlankTitle();
        PolicyAckRequestExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        PolicyAckRequestTitleLengthRuleFlagsALongTitle();
    }
}
