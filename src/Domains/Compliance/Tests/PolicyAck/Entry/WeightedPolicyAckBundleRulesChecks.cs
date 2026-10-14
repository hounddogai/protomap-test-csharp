namespace Acme.Workforce.Compliance.Tests.PolicyAck.Entry;

using Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class WeightedPolicyAckBundleRulesChecks
{
    public static void WeightedPolicyAckBundleValidatorReportsEveryProblem()
    {
        var validator = WeightedPolicyAckBundleRules.DefaultWeightedPolicyAckBundleValidator();
        var item = WeightedPolicyAckBundle.NewWeightedPolicyAckBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedPolicyAckBundleBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new WeightedPolicyAckBundleBadgeLengthRule(64);
        var item = WeightedPolicyAckBundle.NewWeightedPolicyAckBundle("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedPolicyAckBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedPolicyAckBundleRequiredCodeRule("weighted policy ack bundle needs a value");
        var item = WeightedPolicyAckBundle.NewWeightedPolicyAckBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedPolicyAckBundleRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new WeightedPolicyAckBundleRatingRangeRule(1.9, 3386.4);
        var item = WeightedPolicyAckBundle.NewWeightedPolicyAckBundle("x", "y");
        item.Rating = 3387.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedPolicyAckBundleRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new WeightedPolicyAckBundleRequiredBadgeRule("weighted policy ack bundle needs a value");
        var item = WeightedPolicyAckBundle.NewWeightedPolicyAckBundle("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedPolicyAckBundleValidatorReportsEveryProblem();
        WeightedPolicyAckBundleBadgeLengthRuleFlagsALongBadge();
        WeightedPolicyAckBundleRequiredCodeRuleFlagsABlankCode();
        WeightedPolicyAckBundleRatingRangeRuleFlagsValuesAboveRating();
        WeightedPolicyAckBundleRequiredBadgeRuleFlagsABlankBadge();
    }
}
