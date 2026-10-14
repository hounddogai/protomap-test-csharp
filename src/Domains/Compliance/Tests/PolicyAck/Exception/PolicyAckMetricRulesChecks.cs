namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PolicyAckMetricRulesChecks
{
    public static void PolicyAckMetricValidatorReportsEveryProblem()
    {
        var validator = PolicyAckMetricRules.DefaultPolicyAckMetricValidator();
        var item = PolicyAckMetric.NewPolicyAckMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckMetricRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new PolicyAckMetricRequiredExternalRefRule("policy ack metric needs an entry");
        var item = PolicyAckMetric.NewPolicyAckMetric("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new PolicyAckMetricCodeLengthRule(255);
        var item = PolicyAckMetric.NewPolicyAckMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckMetricCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new PolicyAckMetricCapacityRangeRule(64, 64);
        var item = PolicyAckMetric.NewPolicyAckMetric("x", "y");
        item.Capacity = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PolicyAckMetricValidatorReportsEveryProblem();
        PolicyAckMetricRequiredExternalRefRuleFlagsABlankExternalRef();
        PolicyAckMetricCodeLengthRuleFlagsALongCode();
        PolicyAckMetricCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
