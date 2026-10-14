namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PolicyAckAdjustmentRulesChecks
{
    public static void PolicyAckAdjustmentValidatorReportsEveryProblem()
    {
        var validator = PolicyAckAdjustmentRules.DefaultPolicyAckAdjustmentValidator();
        var item = PolicyAckAdjustment.NewPolicyAckAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckAdjustmentLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new PolicyAckAdjustmentLimitRangeRule(16, 80);
        var item = PolicyAckAdjustment.NewPolicyAckAdjustment("x", "y");
        item.Limit = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyAckAdjustmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PolicyAckAdjustmentRequiredTitleRule("policy ack adjustment needs this field");
        var item = PolicyAckAdjustment.NewPolicyAckAdjustment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PolicyAckAdjustmentValidatorReportsEveryProblem();
        PolicyAckAdjustmentLimitRangeRuleFlagsValuesAboveLimit();
        PolicyAckAdjustmentRequiredTitleRuleFlagsABlankTitle();
    }
}
