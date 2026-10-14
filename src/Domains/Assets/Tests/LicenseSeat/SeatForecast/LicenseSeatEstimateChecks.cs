namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatForecast;

using Acme.Workforce.Assets.LicenseSeat.SeatForecast;

/// <summary>Checks of seat forecast code, run by the project's runner.</summary>
public static class LicenseSeatEstimateChecks
{
    public static void LicenseSeatEstimateStartsInOpen()
    {
        var item = LicenseSeatEstimate.NewLicenseSeatEstimate("code-422", "title-761");
        Check.Equal(item.Status, LicenseSeatEstimateStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatEstimateMovesToInReview()
    {
        var item = LicenseSeatEstimate.NewLicenseSeatEstimate("code-422", "title-761");
        try
        {
            item.MoveTo(LicenseSeatEstimateStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatEstimateStatus.InReview, "status");
    }

    public static void LicenseSeatEstimateRejectsMovesFromArchived()
    {
        var item = LicenseSeatEstimate.NewLicenseSeatEstimate("code-422", "title-761");
        item.Status = LicenseSeatEstimateStatus.Archived;
        Check.Fails(() => item.MoveTo(LicenseSeatEstimateStatus.Open), "move");
    }

    public static void RunAll()
    {
        LicenseSeatEstimateStartsInOpen();
        LicenseSeatEstimateMovesToInReview();
        LicenseSeatEstimateRejectsMovesFromArchived();
    }
}
