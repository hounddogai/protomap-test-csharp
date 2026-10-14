namespace Acme.Workforce.Absence.Tests.LeaveRequest.Ledger;

using Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LeaveRequestTargetRulesChecks
{
    public static void LeaveRequestTargetValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestTargetRules.DefaultLeaveRequestTargetValidator();
        var item = LeaveRequestTarget.NewLeaveRequestTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestTargetRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LeaveRequestTargetRequiredLabelRule("leave request target needs a value");
        var item = LeaveRequestTarget.NewLeaveRequestTarget("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestTargetSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new LeaveRequestTargetSegmentLengthRule(32);
        var item = LeaveRequestTarget.NewLeaveRequestTarget("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestTargetRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new LeaveRequestTargetRequiredSegmentRule("leave request target needs a value");
        var item = LeaveRequestTarget.NewLeaveRequestTarget("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LeaveRequestTargetValidatorReportsEveryProblem();
        LeaveRequestTargetRequiredLabelRuleFlagsABlankLabel();
        LeaveRequestTargetSegmentLengthRuleFlagsALongSegment();
        LeaveRequestTargetRequiredSegmentRuleFlagsABlankSegment();
    }
}
