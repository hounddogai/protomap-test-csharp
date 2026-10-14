namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountSchedule;

using Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

/// <summary>Checks of headcount schedule code, run by the project's runner.</summary>
public static class SeasonalHeadcountReservationChecks
{
    public static void SeasonalHeadcountReservationStartsInIdle()
    {
        var item = SeasonalHeadcountReservation.NewSeasonalHeadcountReservation("code-430", "title-691");
        Check.Equal(item.Status, SeasonalHeadcountReservationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalHeadcountReservationMovesToWarming()
    {
        var item = SeasonalHeadcountReservation.NewSeasonalHeadcountReservation("code-430", "title-691");
        try
        {
            item.MoveTo(SeasonalHeadcountReservationStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalHeadcountReservationStatus.Warming, "status");
    }

    public static void SeasonalHeadcountReservationRejectsMovesFromStopped()
    {
        var item = SeasonalHeadcountReservation.NewSeasonalHeadcountReservation("code-430", "title-691");
        item.Status = SeasonalHeadcountReservationStatus.Stopped;
        Check.Fails(() => item.MoveTo(SeasonalHeadcountReservationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SeasonalHeadcountReservationStartsInIdle();
        SeasonalHeadcountReservationMovesToWarming();
        SeasonalHeadcountReservationRejectsMovesFromStopped();
    }
}
