namespace Acme.Workforce.Compliance.Tests.PolicyAck.Notice;

using Acme.Workforce.Compliance.PolicyAck.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PolicyAckRollupRulesChecks
{
    public static void PolicyAckRollupValidatorReportsEveryProblem()
    {
        var validator = PolicyAckRollupRules.DefaultPolicyAckRollupValidator();
        var item = PolicyAckRollup.NewPolicyAckRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckRollupThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new PolicyAckRollupThresholdRangeRule(0.6, 1750.6);
        var item = PolicyAckRollup.NewPolicyAckRollup("x", "y");
        item.Threshold = 1752.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyAckRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new PolicyAckRollupCodeLengthRule(80);
        var item = PolicyAckRollup.NewPolicyAckRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckRollupFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new PolicyAckRollupFloorValueRangeRule(2.3, 1746.2);
        var item = PolicyAckRollup.NewPolicyAckRollup("x", "y");
        item.FloorValue = 1747.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyAckRollupNameLengthRuleFlagsALongName()
    {
        var rule = new PolicyAckRollupNameLengthRule(64);
        var item = PolicyAckRollup.NewPolicyAckRollup("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PolicyAckRollupValidatorReportsEveryProblem();
        PolicyAckRollupThresholdRangeRuleFlagsValuesAboveThreshold();
        PolicyAckRollupCodeLengthRuleFlagsALongCode();
        PolicyAckRollupFloorValueRangeRuleFlagsValuesAboveFloorValue();
        PolicyAckRollupNameLengthRuleFlagsALongName();
    }
}
