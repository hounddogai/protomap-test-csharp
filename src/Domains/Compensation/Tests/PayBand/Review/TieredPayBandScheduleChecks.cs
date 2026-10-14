namespace Acme.Workforce.Compensation.Tests.PayBand.Review;

using Acme.Workforce.Compensation.PayBand.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TieredPayBandScheduleChecks
{
    public static void TieredPayBandScheduleStartsInOpen()
    {
        var item = TieredPayBandSchedule.NewTieredPayBandSchedule("code-710", "name-550");
        Check.Equal(item.Status, TieredPayBandScheduleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPayBandScheduleMovesToReopened()
    {
        var item = TieredPayBandSchedule.NewTieredPayBandSchedule("code-710", "name-550");
        try
        {
            item.MoveTo(TieredPayBandScheduleStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPayBandScheduleStatus.Reopened, "status");
    }

    public static void TieredPayBandScheduleRejectsMovesFromArchived()
    {
        var item = TieredPayBandSchedule.NewTieredPayBandSchedule("code-710", "name-550");
        item.Status = TieredPayBandScheduleStatus.Archived;
        Check.Fails(() => item.MoveTo(TieredPayBandScheduleStatus.Open), "move");
    }

    public static void RunAll()
    {
        TieredPayBandScheduleStartsInOpen();
        TieredPayBandScheduleMovesToReopened();
        TieredPayBandScheduleRejectsMovesFromArchived();
    }
}
