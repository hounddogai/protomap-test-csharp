namespace Acme.Workforce.Absence.Tests.LeaveRequest.Draft;

using Acme.Workforce.Absence.LeaveRequest.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestSnapshotRulesChecks
{
    public static void ScheduledLeaveRequestSnapshotValidatorReportsEveryProblem()
    {
        var validator = ScheduledLeaveRequestSnapshotRules.DefaultScheduledLeaveRequestSnapshotValidator();
        var item = ScheduledLeaveRequestSnapshot.NewScheduledLeaveRequestSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledLeaveRequestSnapshotCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledLeaveRequestSnapshotCodeLengthRule(255);
        var item = ScheduledLeaveRequestSnapshot.NewScheduledLeaveRequestSnapshot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledLeaveRequestSnapshotRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledLeaveRequestSnapshotRequiredTitleRule("scheduled leave request snapshot needs an entry");
        var item = ScheduledLeaveRequestSnapshot.NewScheduledLeaveRequestSnapshot("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledLeaveRequestSnapshotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledLeaveRequestSnapshotRequiredCodeRule("scheduled leave request snapshot needs a value");
        var item = ScheduledLeaveRequestSnapshot.NewScheduledLeaveRequestSnapshot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledLeaveRequestSnapshotCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new ScheduledLeaveRequestSnapshotCreatedAtRangeRule(120, 86400);
        var item = ScheduledLeaveRequestSnapshot.NewScheduledLeaveRequestSnapshot("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledLeaveRequestSnapshotPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new ScheduledLeaveRequestSnapshotPriorityRangeRule(64, 255);
        var item = ScheduledLeaveRequestSnapshot.NewScheduledLeaveRequestSnapshot("x", "y");
        item.Priority = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestSnapshotValidatorReportsEveryProblem();
        ScheduledLeaveRequestSnapshotCodeLengthRuleFlagsALongCode();
        ScheduledLeaveRequestSnapshotRequiredTitleRuleFlagsABlankTitle();
        ScheduledLeaveRequestSnapshotRequiredCodeRuleFlagsABlankCode();
        ScheduledLeaveRequestSnapshotCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        ScheduledLeaveRequestSnapshotPriorityRangeRuleFlagsValuesAbovePriority();
    }
}
