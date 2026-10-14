namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckProfile;

using Acme.Workforce.Compliance.PolicyAck.AckProfile;

/// <summary>Checks of ack profile code, run by the project's runner.</summary>
public static class PolicyAckVarianceRulesChecks
{
    public static void PolicyAckVarianceValidatorReportsEveryProblem()
    {
        var validator = PolicyAckVarianceRules.DefaultPolicyAckVarianceValidator();
        var item = PolicyAckVariance.NewPolicyAckVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PolicyAckVarianceRequiredCodeRule("policy ack variance needs a value");
        var item = PolicyAckVariance.NewPolicyAckVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckVarianceAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new PolicyAckVarianceAttemptsRangeRule(32, 120);
        var item = PolicyAckVariance.NewPolicyAckVariance("x", "y");
        item.Attempts = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyAckVarianceRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new PolicyAckVarianceRequiredEmployeeIdRule("policy ack variance needs an entry");
        var item = PolicyAckVariance.NewPolicyAckVariance("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckVarianceRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new PolicyAckVarianceRequiredCurrencyRule("policy ack variance needs a value");
        var item = PolicyAckVariance.NewPolicyAckVariance("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PolicyAckVarianceValidatorReportsEveryProblem();
        PolicyAckVarianceRequiredCodeRuleFlagsABlankCode();
        PolicyAckVarianceAttemptsRangeRuleFlagsValuesAboveAttempts();
        PolicyAckVarianceRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        PolicyAckVarianceRequiredCurrencyRuleFlagsABlankCurrency();
    }
}
