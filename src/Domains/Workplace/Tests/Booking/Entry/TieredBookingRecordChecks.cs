namespace Acme.Workforce.Workplace.Tests.Booking.Entry;

using Acme.Workforce.Workplace.Booking.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TieredBookingRecordChecks
{
    public static void TieredBookingRecordStartsInCreated()
    {
        var item = TieredBookingRecord.NewTieredBookingRecord("code-399", "name-393");
        Check.Equal(item.Status, TieredBookingRecordStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredBookingRecordMovesToPublished()
    {
        var item = TieredBookingRecord.NewTieredBookingRecord("code-399", "name-393");
        try
        {
            item.MoveTo(TieredBookingRecordStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredBookingRecordStatus.Published, "status");
    }

    public static void TieredBookingRecordRejectsMovesFromRetired()
    {
        var item = TieredBookingRecord.NewTieredBookingRecord("code-399", "name-393");
        item.Status = TieredBookingRecordStatus.Retired;
        Check.Fails(() => item.MoveTo(TieredBookingRecordStatus.Created), "move");
    }

    public static void RunAll()
    {
        TieredBookingRecordStartsInCreated();
        TieredBookingRecordMovesToPublished();
        TieredBookingRecordRejectsMovesFromRetired();
    }
}
