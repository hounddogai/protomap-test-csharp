namespace Acme.Workforce.Absence.Tests.Holiday.Adjustment;

using Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LenientHolidayAdjustmentChecks
{
    public static void LenientHolidayAdjustmentStartsInRequested()
    {
        var item = LenientHolidayAdjustment.NewLenientHolidayAdjustment("code-210", "name-107");
        Check.Equal(item.Status, LenientHolidayAdjustmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientHolidayAdjustmentMovesToExpired()
    {
        var item = LenientHolidayAdjustment.NewLenientHolidayAdjustment("code-210", "name-107");
        try
        {
            item.MoveTo(LenientHolidayAdjustmentStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientHolidayAdjustmentStatus.Expired, "status");
    }

    public static void LenientHolidayAdjustmentRejectsMovesFromExpired()
    {
        var item = LenientHolidayAdjustment.NewLenientHolidayAdjustment("code-210", "name-107");
        item.Status = LenientHolidayAdjustmentStatus.Expired;
        Check.Fails(() => item.MoveTo(LenientHolidayAdjustmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LenientHolidayAdjustmentStartsInRequested();
        LenientHolidayAdjustmentMovesToExpired();
        LenientHolidayAdjustmentRejectsMovesFromExpired();
    }
}
