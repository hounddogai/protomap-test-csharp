namespace Acme.Workforce.Talent.Tests.Mobility.Allocation;

using Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RollingMobilityNoticeChecks
{
    public static void RollingMobilityNoticeStartsInPlanned()
    {
        var item = RollingMobilityNotice.NewRollingMobilityNotice("code-192", "name-894");
        Check.Equal(item.Status, RollingMobilityNoticeStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingMobilityNoticeMovesToCancelled()
    {
        var item = RollingMobilityNotice.NewRollingMobilityNotice("code-192", "name-894");
        try
        {
            item.MoveTo(RollingMobilityNoticeStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingMobilityNoticeStatus.Cancelled, "status");
    }

    public static void RollingMobilityNoticeRejectsMovesFromCancelled()
    {
        var item = RollingMobilityNotice.NewRollingMobilityNotice("code-192", "name-894");
        item.Status = RollingMobilityNoticeStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RollingMobilityNoticeStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RollingMobilityNoticeStartsInPlanned();
        RollingMobilityNoticeMovesToCancelled();
        RollingMobilityNoticeRejectsMovesFromCancelled();
    }
}
