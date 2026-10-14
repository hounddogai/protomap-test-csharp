namespace Acme.Workforce.Compensation.Tests.BonusPool.Exception;

using Acme.Workforce.Compensation.BonusPool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class AdaptiveBonusPoolReservationChecks
{
    public static void AdaptiveBonusPoolReservationStartsInRequested()
    {
        var item = AdaptiveBonusPoolReservation.NewAdaptiveBonusPoolReservation("code-22", "label-944");
        Check.Equal(item.Status, AdaptiveBonusPoolReservationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveBonusPoolReservationMovesToRevoked()
    {
        var item = AdaptiveBonusPoolReservation.NewAdaptiveBonusPoolReservation("code-22", "label-944");
        try
        {
            item.MoveTo(AdaptiveBonusPoolReservationStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveBonusPoolReservationStatus.Revoked, "status");
    }

    public static void AdaptiveBonusPoolReservationRejectsMovesFromExpired()
    {
        var item = AdaptiveBonusPoolReservation.NewAdaptiveBonusPoolReservation("code-22", "label-944");
        item.Status = AdaptiveBonusPoolReservationStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptiveBonusPoolReservationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptiveBonusPoolReservationStartsInRequested();
        AdaptiveBonusPoolReservationMovesToRevoked();
        AdaptiveBonusPoolReservationRejectsMovesFromExpired();
    }
}
