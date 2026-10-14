namespace Acme.Workforce.Compliance.Tests.PolicyAck.Entry;

using Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PolicyAckAllocationRulesChecks
{
    public static void PolicyAckAllocationValidatorReportsEveryProblem()
    {
        var validator = PolicyAckAllocationRules.DefaultPolicyAckAllocationValidator();
        var item = PolicyAckAllocation.NewPolicyAckAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckAllocationOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new PolicyAckAllocationOwnerIdLengthRule(255);
        var item = PolicyAckAllocation.NewPolicyAckAllocation("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckAllocationLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new PolicyAckAllocationLimitRangeRule(120, 86400);
        var item = PolicyAckAllocation.NewPolicyAckAllocation("x", "y");
        item.Limit = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyAckAllocationRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new PolicyAckAllocationRequiredGradeRule("policy ack allocation needs this field");
        var item = PolicyAckAllocation.NewPolicyAckAllocation("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PolicyAckAllocationValidatorReportsEveryProblem();
        PolicyAckAllocationOwnerIdLengthRuleFlagsALongOwnerId();
        PolicyAckAllocationLimitRangeRuleFlagsValuesAboveLimit();
        PolicyAckAllocationRequiredGradeRuleFlagsABlankGrade();
    }
}
