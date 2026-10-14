namespace Acme.Workforce.Compensation.Tests.BonusPool.Slot;

using Acme.Workforce.Compensation.BonusPool.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class FallbackBonusPoolScheduleChecks
{
    public static void FallbackBonusPoolScheduleStartsInDraft()
    {
        var item = FallbackBonusPoolSchedule.NewFallbackBonusPoolSchedule("code-540", "name-995");
        Check.Equal(item.Status, FallbackBonusPoolScheduleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackBonusPoolScheduleMovesToPending()
    {
        var item = FallbackBonusPoolSchedule.NewFallbackBonusPoolSchedule("code-540", "name-995");
        try
        {
            item.MoveTo(FallbackBonusPoolScheduleStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackBonusPoolScheduleStatus.Pending, "status");
    }

    public static void FallbackBonusPoolScheduleRejectsMovesFromClosed()
    {
        var item = FallbackBonusPoolSchedule.NewFallbackBonusPoolSchedule("code-540", "name-995");
        item.Status = FallbackBonusPoolScheduleStatus.Closed;
        Check.Fails(() => item.MoveTo(FallbackBonusPoolScheduleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FallbackBonusPoolScheduleStartsInDraft();
        FallbackBonusPoolScheduleMovesToPending();
        FallbackBonusPoolScheduleRejectsMovesFromClosed();
    }
}
