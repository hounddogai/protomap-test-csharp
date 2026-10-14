namespace Acme.Workforce.Orgdesign.Tests.Span.Ledger;

using Acme.Workforce.Orgdesign.Span.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class TieredSpanScheduleChecks
{
    public static void TieredSpanScheduleStartsInIdle()
    {
        var item = TieredSpanSchedule.NewTieredSpanSchedule("code-728", "name-94");
        Check.Equal(item.Status, TieredSpanScheduleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredSpanScheduleMovesToReady()
    {
        var item = TieredSpanSchedule.NewTieredSpanSchedule("code-728", "name-94");
        try
        {
            item.MoveTo(TieredSpanScheduleStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredSpanScheduleStatus.Ready, "status");
    }

    public static void TieredSpanScheduleRejectsMovesFromStopped()
    {
        var item = TieredSpanSchedule.NewTieredSpanSchedule("code-728", "name-94");
        item.Status = TieredSpanScheduleStatus.Stopped;
        Check.Fails(() => item.MoveTo(TieredSpanScheduleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TieredSpanScheduleStartsInIdle();
        TieredSpanScheduleMovesToReady();
        TieredSpanScheduleRejectsMovesFromStopped();
    }
}
