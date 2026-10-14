namespace Acme.Workforce.Absence.Tests.LeaveRequest.Budget;

using Acme.Workforce.Absence.LeaveRequest.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LeaveRequestRuleChecks
{
    public static void LeaveRequestRuleStartsInIdle()
    {
        var item = LeaveRequestRule.NewLeaveRequestRule("code-782", "label-521");
        Check.Equal(item.Status, LeaveRequestRuleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestRuleMovesToReady()
    {
        var item = LeaveRequestRule.NewLeaveRequestRule("code-782", "label-521");
        try
        {
            item.MoveTo(LeaveRequestRuleStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestRuleStatus.Ready, "status");
    }

    public static void LeaveRequestRuleRejectsMovesFromStopped()
    {
        var item = LeaveRequestRule.NewLeaveRequestRule("code-782", "label-521");
        item.Status = LeaveRequestRuleStatus.Stopped;
        Check.Fails(() => item.MoveTo(LeaveRequestRuleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LeaveRequestRuleStartsInIdle();
        LeaveRequestRuleMovesToReady();
        LeaveRequestRuleRejectsMovesFromStopped();
    }
}
