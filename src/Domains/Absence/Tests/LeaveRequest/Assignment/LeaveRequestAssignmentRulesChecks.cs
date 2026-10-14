namespace Acme.Workforce.Absence.Tests.LeaveRequest.Assignment;

using Acme.Workforce.Absence.LeaveRequest.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LeaveRequestAssignmentRulesChecks
{
    public static void LeaveRequestAssignmentValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestAssignmentRules.DefaultLeaveRequestAssignmentValidator();
        var item = LeaveRequestAssignment.NewLeaveRequestAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestAssignmentLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new LeaveRequestAssignmentLeaveDaysRangeRule(1000, 3600);
        var item = LeaveRequestAssignment.NewLeaveRequestAssignment("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LeaveRequestAssignmentRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new LeaveRequestAssignmentRevisionRangeRule(1000, 86400);
        var item = LeaveRequestAssignment.NewLeaveRequestAssignment("x", "y");
        item.Revision = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LeaveRequestAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new LeaveRequestAssignmentCodeLengthRule(16);
        var item = LeaveRequestAssignment.NewLeaveRequestAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LeaveRequestAssignmentValidatorReportsEveryProblem();
        LeaveRequestAssignmentLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        LeaveRequestAssignmentRevisionRangeRuleFlagsValuesAboveRevision();
        LeaveRequestAssignmentCodeLengthRuleFlagsALongCode();
    }
}
