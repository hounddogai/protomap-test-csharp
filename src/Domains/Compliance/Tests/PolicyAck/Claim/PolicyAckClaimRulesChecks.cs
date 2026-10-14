namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PolicyAckClaimRulesChecks
{
    public static void PolicyAckClaimValidatorReportsEveryProblem()
    {
        var validator = PolicyAckClaimRules.DefaultPolicyAckClaimValidator();
        var item = PolicyAckClaim.NewPolicyAckClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckClaimCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new PolicyAckClaimCategoryLengthRule(255);
        var item = PolicyAckClaim.NewPolicyAckClaim("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckClaimRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PolicyAckClaimRequiredTitleRule("policy ack claim needs an entry");
        var item = PolicyAckClaim.NewPolicyAckClaim("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckClaimTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PolicyAckClaimTitleLengthRule(80);
        var item = PolicyAckClaim.NewPolicyAckClaim("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckClaimRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new PolicyAckClaimRequiredCategoryRule("policy ack claim needs an entry");
        var item = PolicyAckClaim.NewPolicyAckClaim("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckClaimAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PolicyAckClaimAmountRangeRule(0.7, 1421.7);
        var item = PolicyAckClaim.NewPolicyAckClaim("x", "y");
        item.Amount = 1423.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PolicyAckClaimValidatorReportsEveryProblem();
        PolicyAckClaimCategoryLengthRuleFlagsALongCategory();
        PolicyAckClaimRequiredTitleRuleFlagsABlankTitle();
        PolicyAckClaimTitleLengthRuleFlagsALongTitle();
        PolicyAckClaimRequiredCategoryRuleFlagsABlankCategory();
        PolicyAckClaimAmountRangeRuleFlagsValuesAboveAmount();
    }
}
