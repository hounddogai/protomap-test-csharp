namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class PositionScheduleChecks
{
    public static void PositionScheduleStartsInNew()
    {
        var item = PositionSchedule.NewPositionSchedule("code-938", "label-989");
        Check.Equal(item.Status, PositionScheduleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionScheduleMovesToQueued()
    {
        var item = PositionSchedule.NewPositionSchedule("code-938", "label-989");
        try
        {
            item.MoveTo(PositionScheduleStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionScheduleStatus.Queued, "status");
    }

    public static void PositionScheduleRejectsMovesFromFailed()
    {
        var item = PositionSchedule.NewPositionSchedule("code-938", "label-989");
        item.Status = PositionScheduleStatus.Failed;
        Check.Fails(() => item.MoveTo(PositionScheduleStatus.New), "move");
    }

    public static void RunAll()
    {
        PositionScheduleStartsInNew();
        PositionScheduleMovesToQueued();
        PositionScheduleRejectsMovesFromFailed();
    }
}
