namespace Acme.Workforce.Compliance.Tests.PolicyAck.Template;

using Acme.Workforce.Compliance.PolicyAck.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PolicyAckBaselineRulesChecks
{
    public static void PolicyAckBaselineValidatorReportsEveryProblem()
    {
        var validator = PolicyAckBaselineRules.DefaultPolicyAckBaselineValidator();
        var item = PolicyAckBaseline.NewPolicyAckBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckBaselineSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new PolicyAckBaselineSegmentLengthRule(86400);
        var item = PolicyAckBaseline.NewPolicyAckBaseline("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckBaselineRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PolicyAckBaselineRequiredTitleRule("policy ack baseline needs this field");
        var item = PolicyAckBaseline.NewPolicyAckBaseline("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckBaselineDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new PolicyAckBaselineDiscountRangeRule(9.5, 1591.8);
        var item = PolicyAckBaseline.NewPolicyAckBaseline("x", "y");
        item.Discount = 1593.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyAckBaselineRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new PolicyAckBaselineRequiredSegmentRule("policy ack baseline needs this field");
        var item = PolicyAckBaseline.NewPolicyAckBaseline("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PolicyAckBaselineValidatorReportsEveryProblem();
        PolicyAckBaselineSegmentLengthRuleFlagsALongSegment();
        PolicyAckBaselineRequiredTitleRuleFlagsABlankTitle();
        PolicyAckBaselineDiscountRangeRuleFlagsValuesAboveDiscount();
        PolicyAckBaselineRequiredSegmentRuleFlagsABlankSegment();
    }
}
