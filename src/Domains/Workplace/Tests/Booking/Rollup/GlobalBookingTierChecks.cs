namespace Acme.Workforce.Workplace.Tests.Booking.Rollup;

using Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class GlobalBookingTierChecks
{
    public static void GlobalBookingTierStartsInRequested()
    {
        var item = GlobalBookingTier.NewGlobalBookingTier("code-106", "title-494");
        Check.Equal(item.Status, GlobalBookingTierStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalBookingTierMovesToGranted()
    {
        var item = GlobalBookingTier.NewGlobalBookingTier("code-106", "title-494");
        try
        {
            item.MoveTo(GlobalBookingTierStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalBookingTierStatus.Granted, "status");
    }

    public static void GlobalBookingTierRejectsMovesFromExpired()
    {
        var item = GlobalBookingTier.NewGlobalBookingTier("code-106", "title-494");
        item.Status = GlobalBookingTierStatus.Expired;
        Check.Fails(() => item.MoveTo(GlobalBookingTierStatus.Requested), "move");
    }

    public static void RunAll()
    {
        GlobalBookingTierStartsInRequested();
        GlobalBookingTierMovesToGranted();
        GlobalBookingTierRejectsMovesFromExpired();
    }
}
