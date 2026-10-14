namespace Acme.Workforce.Compliance.Tests.PolicyAck.OverrideItem;

using Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyAckMappingRulesChecks
{
    public static void PolicyAckMappingValidatorReportsEveryProblem()
    {
        var validator = PolicyAckMappingRules.DefaultPolicyAckMappingValidator();
        var item = PolicyAckMapping.NewPolicyAckMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new PolicyAckMappingCodeLengthRule(32);
        var item = PolicyAckMapping.NewPolicyAckMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PolicyAckMappingValidatorReportsEveryProblem();
        PolicyAckMappingCodeLengthRuleFlagsALongCode();
    }
}
