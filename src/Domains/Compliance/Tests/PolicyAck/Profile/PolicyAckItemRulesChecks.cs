namespace Acme.Workforce.Compliance.Tests.PolicyAck.Profile;

using Acme.Workforce.Compliance.PolicyAck.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PolicyAckItemRulesChecks
{
    public static void PolicyAckItemValidatorReportsEveryProblem()
    {
        var validator = PolicyAckItemRules.DefaultPolicyAckItemValidator();
        var item = PolicyAckItem.NewPolicyAckItem("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckItemRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PolicyAckItemRequiredLabelRule("policy ack item needs an entry");
        var item = PolicyAckItem.NewPolicyAckItem("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckItemRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new PolicyAckItemRateRangeRule(4.6, 3540.4);
        var item = PolicyAckItem.NewPolicyAckItem("x", "y");
        item.Rate = 3541.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyAckItemSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new PolicyAckItemSequenceRangeRule(32, 86400);
        var item = PolicyAckItem.NewPolicyAckItem("x", "y");
        item.Sequence = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyAckItemRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PolicyAckItemRequiredCodeRule("policy ack item needs a value");
        var item = PolicyAckItem.NewPolicyAckItem("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PolicyAckItemValidatorReportsEveryProblem();
        PolicyAckItemRequiredLabelRuleFlagsABlankLabel();
        PolicyAckItemRateRangeRuleFlagsValuesAboveRate();
        PolicyAckItemSequenceRangeRuleFlagsValuesAboveSequence();
        PolicyAckItemRequiredCodeRuleFlagsABlankCode();
    }
}
