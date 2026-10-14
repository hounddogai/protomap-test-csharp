namespace Acme.Workforce.Orgdesign.Tests.Headcount.RecordItem;

using Acme.Workforce.Orgdesign.Headcount.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class HeadcountBatchChecks
{
    public static void HeadcountBatchStartsInOpen()
    {
        var item = HeadcountBatch.NewHeadcountBatch("code-460", "name-204");
        Check.Equal(item.Status, HeadcountBatchStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountBatchMovesToArchived()
    {
        var item = HeadcountBatch.NewHeadcountBatch("code-460", "name-204");
        try
        {
            item.MoveTo(HeadcountBatchStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountBatchStatus.Archived, "status");
    }

    public static void HeadcountBatchRejectsMovesFromArchived()
    {
        var item = HeadcountBatch.NewHeadcountBatch("code-460", "name-204");
        item.Status = HeadcountBatchStatus.Archived;
        Check.Fails(() => item.MoveTo(HeadcountBatchStatus.Open), "move");
    }

    public static void RunAll()
    {
        HeadcountBatchStartsInOpen();
        HeadcountBatchMovesToArchived();
        HeadcountBatchRejectsMovesFromArchived();
    }
}
