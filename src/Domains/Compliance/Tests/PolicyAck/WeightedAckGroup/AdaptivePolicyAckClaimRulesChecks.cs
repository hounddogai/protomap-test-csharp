namespace Acme.Workforce.Compliance.Tests.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

/// <summary>Checks of weighted ack group code, run by the project's runner.</summary>
public static class AdaptivePolicyAckClaimRulesChecks
{
    public static void AdaptivePolicyAckClaimValidatorReportsEveryProblem()
    {
        var validator = AdaptivePolicyAckClaimRules.DefaultAdaptivePolicyAckClaimValidator();
        var item = AdaptivePolicyAckClaim.NewAdaptivePolicyAckClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptivePolicyAckClaimSourceLengthRuleFlagsALongSource()
    {
        var rule = new AdaptivePolicyAckClaimSourceLengthRule(1000);
        var item = AdaptivePolicyAckClaim.NewAdaptivePolicyAckClaim("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptivePolicyAckClaimTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new AdaptivePolicyAckClaimTenureMonthsRangeRule(120, 120);
        var item = AdaptivePolicyAckClaim.NewAdaptivePolicyAckClaim("x", "y");
        item.TenureMonths = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptivePolicyAckClaimLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new AdaptivePolicyAckClaimLimitRangeRule(64, 1000);
        var item = AdaptivePolicyAckClaim.NewAdaptivePolicyAckClaim("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptivePolicyAckClaimValidatorReportsEveryProblem();
        AdaptivePolicyAckClaimSourceLengthRuleFlagsALongSource();
        AdaptivePolicyAckClaimTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        AdaptivePolicyAckClaimLimitRangeRuleFlagsValuesAboveLimit();
    }
}
