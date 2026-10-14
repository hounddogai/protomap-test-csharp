namespace Acme.Workforce.Assets.Tests.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

/// <summary>Checks of strict seat reservation code, run by the project's runner.</summary>
public static class PendingLicenseSeatSlotChecks
{
    public static void PendingLicenseSeatSlotStartsInOpen()
    {
        var item = PendingLicenseSeatSlot.NewPendingLicenseSeatSlot("code-752", "label-510");
        Check.Equal(item.Status, PendingLicenseSeatSlotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingLicenseSeatSlotMovesToInReview()
    {
        var item = PendingLicenseSeatSlot.NewPendingLicenseSeatSlot("code-752", "label-510");
        try
        {
            item.MoveTo(PendingLicenseSeatSlotStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingLicenseSeatSlotStatus.InReview, "status");
    }

    public static void PendingLicenseSeatSlotRejectsMovesFromArchived()
    {
        var item = PendingLicenseSeatSlot.NewPendingLicenseSeatSlot("code-752", "label-510");
        item.Status = PendingLicenseSeatSlotStatus.Archived;
        Check.Fails(() => item.MoveTo(PendingLicenseSeatSlotStatus.Open), "move");
    }

    public static void RunAll()
    {
        PendingLicenseSeatSlotStartsInOpen();
        PendingLicenseSeatSlotMovesToInReview();
        PendingLicenseSeatSlotRejectsMovesFromArchived();
    }
}
