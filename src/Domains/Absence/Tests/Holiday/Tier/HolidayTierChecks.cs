namespace Acme.Workforce.Absence.Tests.Holiday.Tier;

using Acme.Workforce.Absence.Holiday.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HolidayTierChecks
{
    public static void HolidayTierStartsInProposed()
    {
        var item = HolidayTier.NewHolidayTier("code-192", "title-135");
        Check.Equal(item.Status, HolidayTierStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayTierMovesToRejected()
    {
        var item = HolidayTier.NewHolidayTier("code-192", "title-135");
        try
        {
            item.MoveTo(HolidayTierStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayTierStatus.Rejected, "status");
    }

    public static void HolidayTierRejectsMovesFromWithdrawn()
    {
        var item = HolidayTier.NewHolidayTier("code-192", "title-135");
        item.Status = HolidayTierStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(HolidayTierStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        HolidayTierStartsInProposed();
        HolidayTierMovesToRejected();
        HolidayTierRejectsMovesFromWithdrawn();
    }
}
