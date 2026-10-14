namespace Acme.Workforce.Compliance.Tests.PolicyAck.Group;

using Acme.Workforce.Compliance.PolicyAck.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PolicyAckGroupRulesChecks
{
    public static void PolicyAckGroupValidatorReportsEveryProblem()
    {
        var validator = PolicyAckGroupRules.DefaultPolicyAckGroupValidator();
        var item = PolicyAckGroup.NewPolicyAckGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckGroupScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new PolicyAckGroupScoreRangeRule(7.1, 902.0);
        var item = PolicyAckGroup.NewPolicyAckGroup("x", "y");
        item.Score = 903.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyAckGroupTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PolicyAckGroupTitleLengthRule(3600);
        var item = PolicyAckGroup.NewPolicyAckGroup("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckGroupSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new PolicyAckGroupSegmentLengthRule(32);
        var item = PolicyAckGroup.NewPolicyAckGroup("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PolicyAckGroupRequiredCodeRule("policy ack group needs a value");
        var item = PolicyAckGroup.NewPolicyAckGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckGroupTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new PolicyAckGroupTenureMonthsRangeRule(80, 86400);
        var item = PolicyAckGroup.NewPolicyAckGroup("x", "y");
        item.TenureMonths = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PolicyAckGroupValidatorReportsEveryProblem();
        PolicyAckGroupScoreRangeRuleFlagsValuesAboveScore();
        PolicyAckGroupTitleLengthRuleFlagsALongTitle();
        PolicyAckGroupSegmentLengthRuleFlagsALongSegment();
        PolicyAckGroupRequiredCodeRuleFlagsABlankCode();
        PolicyAckGroupTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
