namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class AdaptiveCostModelReservationChecks
{
    public static void AdaptiveCostModelReservationStartsInRequested()
    {
        var item = AdaptiveCostModelReservation.NewAdaptiveCostModelReservation("code-735", "name-977");
        Check.Equal(item.Status, AdaptiveCostModelReservationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveCostModelReservationMovesToRevoked()
    {
        var item = AdaptiveCostModelReservation.NewAdaptiveCostModelReservation("code-735", "name-977");
        try
        {
            item.MoveTo(AdaptiveCostModelReservationStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveCostModelReservationStatus.Revoked, "status");
    }

    public static void AdaptiveCostModelReservationRejectsMovesFromExpired()
    {
        var item = AdaptiveCostModelReservation.NewAdaptiveCostModelReservation("code-735", "name-977");
        item.Status = AdaptiveCostModelReservationStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptiveCostModelReservationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptiveCostModelReservationStartsInRequested();
        AdaptiveCostModelReservationMovesToRevoked();
        AdaptiveCostModelReservationRejectsMovesFromExpired();
    }
}
