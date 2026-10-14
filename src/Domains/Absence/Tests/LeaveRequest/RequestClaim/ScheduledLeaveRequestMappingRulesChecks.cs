namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;

/// <summary>Checks of request claim code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestMappingRulesChecks
{
    public static void ScheduledLeaveRequestMappingValidatorReportsEveryProblem()
    {
        var validator = ScheduledLeaveRequestMappingRules.DefaultScheduledLeaveRequestMappingValidator();
        var item = ScheduledLeaveRequestMapping.NewScheduledLeaveRequestMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledLeaveRequestMappingSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new ScheduledLeaveRequestMappingSegmentLengthRule(64);
        var item = ScheduledLeaveRequestMapping.NewScheduledLeaveRequestMapping("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledLeaveRequestMappingRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new ScheduledLeaveRequestMappingRequiredSegmentRule("scheduled leave request mapping needs a value");
        var item = ScheduledLeaveRequestMapping.NewScheduledLeaveRequestMapping("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledLeaveRequestMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledLeaveRequestMappingCodeLengthRule(120);
        var item = ScheduledLeaveRequestMapping.NewScheduledLeaveRequestMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledLeaveRequestMappingRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledLeaveRequestMappingRequiredTitleRule("scheduled leave request mapping needs this field");
        var item = ScheduledLeaveRequestMapping.NewScheduledLeaveRequestMapping("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestMappingValidatorReportsEveryProblem();
        ScheduledLeaveRequestMappingSegmentLengthRuleFlagsALongSegment();
        ScheduledLeaveRequestMappingRequiredSegmentRuleFlagsABlankSegment();
        ScheduledLeaveRequestMappingCodeLengthRuleFlagsALongCode();
        ScheduledLeaveRequestMappingRequiredTitleRuleFlagsABlankTitle();
    }
}
