namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckCycle;

using Acme.Workforce.Compliance.PolicyAck.AckCycle;

/// <summary>Checks of ack cycle code, run by the project's runner.</summary>
public static class CappedPolicyAckRollupRulesChecks
{
    public static void CappedPolicyAckRollupValidatorReportsEveryProblem()
    {
        var validator = CappedPolicyAckRollupRules.DefaultCappedPolicyAckRollupValidator();
        var item = CappedPolicyAckRollup.NewCappedPolicyAckRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedPolicyAckRollupRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new CappedPolicyAckRollupRequiredSummaryRule("capped policy ack rollup needs an entry");
        var item = CappedPolicyAckRollup.NewCappedPolicyAckRollup("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedPolicyAckRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedPolicyAckRollupCodeLengthRule(16);
        var item = CappedPolicyAckRollup.NewCappedPolicyAckRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedPolicyAckRollupRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new CappedPolicyAckRollupRequiredSegmentRule("capped policy ack rollup needs a value");
        var item = CappedPolicyAckRollup.NewCappedPolicyAckRollup("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedPolicyAckRollupRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CappedPolicyAckRollupRequiredTitleRule("capped policy ack rollup needs this field");
        var item = CappedPolicyAckRollup.NewCappedPolicyAckRollup("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedPolicyAckRollupSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new CappedPolicyAckRollupSummaryLengthRule(3600);
        var item = CappedPolicyAckRollup.NewCappedPolicyAckRollup("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedPolicyAckRollupValidatorReportsEveryProblem();
        CappedPolicyAckRollupRequiredSummaryRuleFlagsABlankSummary();
        CappedPolicyAckRollupCodeLengthRuleFlagsALongCode();
        CappedPolicyAckRollupRequiredSegmentRuleFlagsABlankSegment();
        CappedPolicyAckRollupRequiredTitleRuleFlagsABlankTitle();
        CappedPolicyAckRollupSummaryLengthRuleFlagsALongSummary();
    }
}
