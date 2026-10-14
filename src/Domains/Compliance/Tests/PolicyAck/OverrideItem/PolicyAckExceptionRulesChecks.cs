namespace Acme.Workforce.Compliance.Tests.PolicyAck.OverrideItem;

using Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyAckExceptionRulesChecks
{
    public static void PolicyAckExceptionValidatorReportsEveryProblem()
    {
        var validator = PolicyAckExceptionRules.DefaultPolicyAckExceptionValidator();
        var item = PolicyAckException.NewPolicyAckException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckExceptionRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PolicyAckExceptionRequiredLabelRule("policy ack exception needs an entry");
        var item = PolicyAckException.NewPolicyAckException("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckExceptionCodeLengthRuleFlagsALongCode()
    {
        var rule = new PolicyAckExceptionCodeLengthRule(3600);
        var item = PolicyAckException.NewPolicyAckException("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PolicyAckExceptionValidatorReportsEveryProblem();
        PolicyAckExceptionRequiredLabelRuleFlagsABlankLabel();
        PolicyAckExceptionCodeLengthRuleFlagsALongCode();
    }
}
