namespace Acme.Workforce.Contractors.Tests.Rate.RecordItem;

using Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RateRecordChecks
{
    public static void RateRecordStartsInCreated()
    {
        var item = RateRecord.NewRateRecord("code-196", "title-928");
        Check.Equal(item.Status, RateRecordStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateRecordMovesToRetired()
    {
        var item = RateRecord.NewRateRecord("code-196", "title-928");
        try
        {
            item.MoveTo(RateRecordStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateRecordStatus.Retired, "status");
    }

    public static void RateRecordRejectsMovesFromRetired()
    {
        var item = RateRecord.NewRateRecord("code-196", "title-928");
        item.Status = RateRecordStatus.Retired;
        Check.Fails(() => item.MoveTo(RateRecordStatus.Created), "move");
    }

    public static void RunAll()
    {
        RateRecordStartsInCreated();
        RateRecordMovesToRetired();
        RateRecordRejectsMovesFromRetired();
    }
}
