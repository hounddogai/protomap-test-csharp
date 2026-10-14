namespace Acme.Workforce.Compensation.Tests.BonusPool.Schedule;

using Acme.Workforce.Compensation.BonusPool.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class BonusPoolScheduleChecks
{
    public static void BonusPoolScheduleStartsInProposed()
    {
        var item = BonusPoolSchedule.NewBonusPoolSchedule("code-576", "title-279");
        Check.Equal(item.Status, BonusPoolScheduleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolScheduleMovesToApproved()
    {
        var item = BonusPoolSchedule.NewBonusPoolSchedule("code-576", "title-279");
        try
        {
            item.MoveTo(BonusPoolScheduleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolScheduleStatus.Approved, "status");
    }

    public static void BonusPoolScheduleRejectsMovesFromWithdrawn()
    {
        var item = BonusPoolSchedule.NewBonusPoolSchedule("code-576", "title-279");
        item.Status = BonusPoolScheduleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BonusPoolScheduleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BonusPoolScheduleStartsInProposed();
        BonusPoolScheduleMovesToApproved();
        BonusPoolScheduleRejectsMovesFromWithdrawn();
    }
}
