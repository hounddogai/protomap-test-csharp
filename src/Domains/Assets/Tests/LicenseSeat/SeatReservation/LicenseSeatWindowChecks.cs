namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatReservation;

using Acme.Workforce.Assets.LicenseSeat.SeatReservation;

/// <summary>Checks of seat reservation code, run by the project's runner.</summary>
public static class LicenseSeatWindowChecks
{
    public static void LicenseSeatWindowStartsInOpen()
    {
        var item = LicenseSeatWindow.NewLicenseSeatWindow("code-507", "title-789");
        Check.Equal(item.Status, LicenseSeatWindowStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatWindowMovesToInReview()
    {
        var item = LicenseSeatWindow.NewLicenseSeatWindow("code-507", "title-789");
        try
        {
            item.MoveTo(LicenseSeatWindowStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatWindowStatus.InReview, "status");
    }

    public static void LicenseSeatWindowRejectsMovesFromArchived()
    {
        var item = LicenseSeatWindow.NewLicenseSeatWindow("code-507", "title-789");
        item.Status = LicenseSeatWindowStatus.Archived;
        Check.Fails(() => item.MoveTo(LicenseSeatWindowStatus.Open), "move");
    }

    public static void RunAll()
    {
        LicenseSeatWindowStartsInOpen();
        LicenseSeatWindowMovesToInReview();
        LicenseSeatWindowRejectsMovesFromArchived();
    }
}
