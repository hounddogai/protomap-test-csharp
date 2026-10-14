namespace Acme.Workforce.Absence.Tests.Holiday.Tier;

using Acme.Workforce.Absence.Holiday.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HolidayClaimChecks
{
    public static void HolidayClaimStartsInDraft()
    {
        var item = HolidayClaim.NewHolidayClaim("code-386", "name-637");
        Check.Equal(item.Status, HolidayClaimStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayClaimMovesToActive()
    {
        var item = HolidayClaim.NewHolidayClaim("code-386", "name-637");
        try
        {
            item.MoveTo(HolidayClaimStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayClaimStatus.Active, "status");
    }

    public static void HolidayClaimRejectsMovesFromClosed()
    {
        var item = HolidayClaim.NewHolidayClaim("code-386", "name-637");
        item.Status = HolidayClaimStatus.Closed;
        Check.Fails(() => item.MoveTo(HolidayClaimStatus.Draft), "move");
    }

    public static void RunAll()
    {
        HolidayClaimStartsInDraft();
        HolidayClaimMovesToActive();
        HolidayClaimRejectsMovesFromClosed();
    }
}
