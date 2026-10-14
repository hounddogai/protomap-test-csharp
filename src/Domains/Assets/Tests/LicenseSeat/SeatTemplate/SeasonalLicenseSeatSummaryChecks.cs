namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatTemplate;

using Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

/// <summary>Checks of seat template code, run by the project's runner.</summary>
public static class SeasonalLicenseSeatSummaryChecks
{
    public static void SeasonalLicenseSeatSummaryStartsInIdle()
    {
        var item = SeasonalLicenseSeatSummary.NewSeasonalLicenseSeatSummary("code-649", "title-932");
        Check.Equal(item.Status, SeasonalLicenseSeatSummaryStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalLicenseSeatSummaryMovesToWarming()
    {
        var item = SeasonalLicenseSeatSummary.NewSeasonalLicenseSeatSummary("code-649", "title-932");
        try
        {
            item.MoveTo(SeasonalLicenseSeatSummaryStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalLicenseSeatSummaryStatus.Warming, "status");
    }

    public static void SeasonalLicenseSeatSummaryRejectsMovesFromStopped()
    {
        var item = SeasonalLicenseSeatSummary.NewSeasonalLicenseSeatSummary("code-649", "title-932");
        item.Status = SeasonalLicenseSeatSummaryStatus.Stopped;
        Check.Fails(() => item.MoveTo(SeasonalLicenseSeatSummaryStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SeasonalLicenseSeatSummaryStartsInIdle();
        SeasonalLicenseSeatSummaryMovesToWarming();
        SeasonalLicenseSeatSummaryRejectsMovesFromStopped();
    }
}
