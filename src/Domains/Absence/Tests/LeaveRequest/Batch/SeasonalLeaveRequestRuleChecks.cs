namespace Acme.Workforce.Absence.Tests.LeaveRequest.Batch;

using Acme.Workforce.Absence.LeaveRequest.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SeasonalLeaveRequestRuleChecks
{
    public static void SeasonalLeaveRequestRuleStartsInPlanned()
    {
        var item = SeasonalLeaveRequestRule.NewSeasonalLeaveRequestRule("code-620", "label-991");
        Check.Equal(item.Status, SeasonalLeaveRequestRuleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalLeaveRequestRuleMovesToScheduled()
    {
        var item = SeasonalLeaveRequestRule.NewSeasonalLeaveRequestRule("code-620", "label-991");
        try
        {
            item.MoveTo(SeasonalLeaveRequestRuleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalLeaveRequestRuleStatus.Scheduled, "status");
    }

    public static void SeasonalLeaveRequestRuleRejectsMovesFromCancelled()
    {
        var item = SeasonalLeaveRequestRule.NewSeasonalLeaveRequestRule("code-620", "label-991");
        item.Status = SeasonalLeaveRequestRuleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SeasonalLeaveRequestRuleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SeasonalLeaveRequestRuleStartsInPlanned();
        SeasonalLeaveRequestRuleMovesToScheduled();
        SeasonalLeaveRequestRuleRejectsMovesFromCancelled();
    }
}
