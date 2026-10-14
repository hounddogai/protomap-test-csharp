namespace Acme.Workforce.Compensation.Tests.PayBand.Preference;

using Acme.Workforce.Compensation.PayBand.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PayBandReservationChecks
{
    public static void PayBandReservationStartsInProposed()
    {
        var item = PayBandReservation.NewPayBandReservation("code-76", "name-429");
        Check.Equal(item.Status, PayBandReservationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandReservationMovesToApproved()
    {
        var item = PayBandReservation.NewPayBandReservation("code-76", "name-429");
        try
        {
            item.MoveTo(PayBandReservationStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandReservationStatus.Approved, "status");
    }

    public static void PayBandReservationRejectsMovesFromWithdrawn()
    {
        var item = PayBandReservation.NewPayBandReservation("code-76", "name-429");
        item.Status = PayBandReservationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PayBandReservationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PayBandReservationStartsInProposed();
        PayBandReservationMovesToApproved();
        PayBandReservationRejectsMovesFromWithdrawn();
    }
}
