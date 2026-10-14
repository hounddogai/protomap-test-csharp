namespace Acme.Workforce.Orgdesign.Tests.Span.Schedule;

using Acme.Workforce.Orgdesign.Span.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ManualSpanScheduleChecks
{
    public static void ManualSpanScheduleStartsInProposed()
    {
        var item = ManualSpanSchedule.NewManualSpanSchedule("code-869", "title-276");
        Check.Equal(item.Status, ManualSpanScheduleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualSpanScheduleMovesToApproved()
    {
        var item = ManualSpanSchedule.NewManualSpanSchedule("code-869", "title-276");
        try
        {
            item.MoveTo(ManualSpanScheduleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualSpanScheduleStatus.Approved, "status");
    }

    public static void ManualSpanScheduleRejectsMovesFromWithdrawn()
    {
        var item = ManualSpanSchedule.NewManualSpanSchedule("code-869", "title-276");
        item.Status = ManualSpanScheduleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ManualSpanScheduleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ManualSpanScheduleStartsInProposed();
        ManualSpanScheduleMovesToApproved();
        ManualSpanScheduleRejectsMovesFromWithdrawn();
    }
}
