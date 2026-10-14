namespace Acme.Workforce.Compliance.Tests.PolicyAck.Schedule;

using Acme.Workforce.Compliance.PolicyAck.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TieredPolicyAckScheduleChecks
{
    public static void TieredPolicyAckScheduleStartsInProposed()
    {
        var item = TieredPolicyAckSchedule.NewTieredPolicyAckSchedule("code-46", "title-319");
        Check.Equal(item.Status, TieredPolicyAckScheduleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPolicyAckScheduleMovesToApproved()
    {
        var item = TieredPolicyAckSchedule.NewTieredPolicyAckSchedule("code-46", "title-319");
        try
        {
            item.MoveTo(TieredPolicyAckScheduleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPolicyAckScheduleStatus.Approved, "status");
    }

    public static void TieredPolicyAckScheduleRejectsMovesFromWithdrawn()
    {
        var item = TieredPolicyAckSchedule.NewTieredPolicyAckSchedule("code-46", "title-319");
        item.Status = TieredPolicyAckScheduleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TieredPolicyAckScheduleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TieredPolicyAckScheduleStartsInProposed();
        TieredPolicyAckScheduleMovesToApproved();
        TieredPolicyAckScheduleRejectsMovesFromWithdrawn();
    }
}
