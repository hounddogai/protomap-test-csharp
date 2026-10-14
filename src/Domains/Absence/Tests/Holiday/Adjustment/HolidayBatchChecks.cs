namespace Acme.Workforce.Absence.Tests.Holiday.Adjustment;

using Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HolidayBatchChecks
{
    public static void HolidayBatchStartsInRequested()
    {
        var item = HolidayBatch.NewHolidayBatch("code-731", "title-642");
        Check.Equal(item.Status, HolidayBatchStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayBatchMovesToGranted()
    {
        var item = HolidayBatch.NewHolidayBatch("code-731", "title-642");
        try
        {
            item.MoveTo(HolidayBatchStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayBatchStatus.Granted, "status");
    }

    public static void HolidayBatchRejectsMovesFromExpired()
    {
        var item = HolidayBatch.NewHolidayBatch("code-731", "title-642");
        item.Status = HolidayBatchStatus.Expired;
        Check.Fails(() => item.MoveTo(HolidayBatchStatus.Requested), "move");
    }

    public static void RunAll()
    {
        HolidayBatchStartsInRequested();
        HolidayBatchMovesToGranted();
        HolidayBatchRejectsMovesFromExpired();
    }
}
