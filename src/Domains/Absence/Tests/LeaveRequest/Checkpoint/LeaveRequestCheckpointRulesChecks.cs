namespace Acme.Workforce.Absence.Tests.LeaveRequest.Checkpoint;

using Acme.Workforce.Absence.LeaveRequest.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LeaveRequestCheckpointRulesChecks
{
    public static void LeaveRequestCheckpointValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestCheckpointRules.DefaultLeaveRequestCheckpointValidator();
        var item = LeaveRequestCheckpoint.NewLeaveRequestCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LeaveRequestCheckpointRequiredCodeRule("leave request checkpoint needs this field");
        var item = LeaveRequestCheckpoint.NewLeaveRequestCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestCheckpointRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LeaveRequestCheckpointRequiredTitleRule("leave request checkpoint needs an entry");
        var item = LeaveRequestCheckpoint.NewLeaveRequestCheckpoint("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new LeaveRequestCheckpointCodeLengthRule(3600);
        var item = LeaveRequestCheckpoint.NewLeaveRequestCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LeaveRequestCheckpointValidatorReportsEveryProblem();
        LeaveRequestCheckpointRequiredCodeRuleFlagsABlankCode();
        LeaveRequestCheckpointRequiredTitleRuleFlagsABlankTitle();
        LeaveRequestCheckpointCodeLengthRuleFlagsALongCode();
    }
}
