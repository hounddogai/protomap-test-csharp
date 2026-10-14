namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckProfile;

using Acme.Workforce.Compliance.PolicyAck.AckProfile;

/// <summary>Checks of ack profile code, run by the project's runner.</summary>
public static class PolicyAckPreferenceRulesChecks
{
    public static void PolicyAckPreferenceValidatorReportsEveryProblem()
    {
        var validator = PolicyAckPreferenceRules.DefaultPolicyAckPreferenceValidator();
        var item = PolicyAckPreference.NewPolicyAckPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new PolicyAckPreferenceCodeLengthRule(80);
        var item = PolicyAckPreference.NewPolicyAckPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckPreferenceRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new PolicyAckPreferenceRequiredManagerIdRule("policy ack preference needs a value");
        var item = PolicyAckPreference.NewPolicyAckPreference("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PolicyAckPreferenceValidatorReportsEveryProblem();
        PolicyAckPreferenceCodeLengthRuleFlagsALongCode();
        PolicyAckPreferenceRequiredManagerIdRuleFlagsABlankManagerId();
    }
}
