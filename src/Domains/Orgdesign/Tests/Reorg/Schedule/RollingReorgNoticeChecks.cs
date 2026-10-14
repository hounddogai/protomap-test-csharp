namespace Acme.Workforce.Orgdesign.Tests.Reorg.Schedule;

using Acme.Workforce.Orgdesign.Reorg.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RollingReorgNoticeChecks
{
    public static void RollingReorgNoticeStartsInOpen()
    {
        var item = RollingReorgNotice.NewRollingReorgNotice("code-717", "label-6");
        Check.Equal(item.Status, RollingReorgNoticeStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingReorgNoticeMovesToReopened()
    {
        var item = RollingReorgNotice.NewRollingReorgNotice("code-717", "label-6");
        try
        {
            item.MoveTo(RollingReorgNoticeStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingReorgNoticeStatus.Reopened, "status");
    }

    public static void RollingReorgNoticeRejectsMovesFromArchived()
    {
        var item = RollingReorgNotice.NewRollingReorgNotice("code-717", "label-6");
        item.Status = RollingReorgNoticeStatus.Archived;
        Check.Fails(() => item.MoveTo(RollingReorgNoticeStatus.Open), "move");
    }

    public static void RunAll()
    {
        RollingReorgNoticeStartsInOpen();
        RollingReorgNoticeMovesToReopened();
        RollingReorgNoticeRejectsMovesFromArchived();
    }
}
