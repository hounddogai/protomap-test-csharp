namespace Acme.Workforce.Workplace.Tests.Booking.Rollup;

using Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LegacyBookingRollupChecks
{
    public static void LegacyBookingRollupStartsInProposed()
    {
        var item = LegacyBookingRollup.NewLegacyBookingRollup("code-960", "title-565");
        Check.Equal(item.Status, LegacyBookingRollupStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyBookingRollupMovesToRejected()
    {
        var item = LegacyBookingRollup.NewLegacyBookingRollup("code-960", "title-565");
        try
        {
            item.MoveTo(LegacyBookingRollupStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyBookingRollupStatus.Rejected, "status");
    }

    public static void LegacyBookingRollupRejectsMovesFromWithdrawn()
    {
        var item = LegacyBookingRollup.NewLegacyBookingRollup("code-960", "title-565");
        item.Status = LegacyBookingRollupStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LegacyBookingRollupStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LegacyBookingRollupStartsInProposed();
        LegacyBookingRollupMovesToRejected();
        LegacyBookingRollupRejectsMovesFromWithdrawn();
    }
}
