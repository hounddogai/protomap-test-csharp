namespace Acme.Workforce.Orgdesign.Tests.Headcount.RecordItem;

using Acme.Workforce.Orgdesign.Headcount.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class HeadcountRecordChecks
{
    public static void HeadcountRecordStartsInCreated()
    {
        var item = HeadcountRecord.NewHeadcountRecord("code-849", "name-289");
        Check.Equal(item.Status, HeadcountRecordStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountRecordMovesToRetired()
    {
        var item = HeadcountRecord.NewHeadcountRecord("code-849", "name-289");
        try
        {
            item.MoveTo(HeadcountRecordStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountRecordStatus.Retired, "status");
    }

    public static void HeadcountRecordRejectsMovesFromRetired()
    {
        var item = HeadcountRecord.NewHeadcountRecord("code-849", "name-289");
        item.Status = HeadcountRecordStatus.Retired;
        Check.Fails(() => item.MoveTo(HeadcountRecordStatus.Created), "move");
    }

    public static void RunAll()
    {
        HeadcountRecordStartsInCreated();
        HeadcountRecordMovesToRetired();
        HeadcountRecordRejectsMovesFromRetired();
    }
}
