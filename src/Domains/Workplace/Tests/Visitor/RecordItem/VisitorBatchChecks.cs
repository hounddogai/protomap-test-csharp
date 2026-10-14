namespace Acme.Workforce.Workplace.Tests.Visitor.RecordItem;

using Acme.Workforce.Workplace.Visitor.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class VisitorBatchChecks
{
    public static void VisitorBatchStartsInNew()
    {
        var item = VisitorBatch.NewVisitorBatch("code-361", "title-289");
        Check.Equal(item.Status, VisitorBatchStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorBatchMovesToDone()
    {
        var item = VisitorBatch.NewVisitorBatch("code-361", "title-289");
        try
        {
            item.MoveTo(VisitorBatchStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorBatchStatus.Done, "status");
    }

    public static void VisitorBatchRejectsMovesFromFailed()
    {
        var item = VisitorBatch.NewVisitorBatch("code-361", "title-289");
        item.Status = VisitorBatchStatus.Failed;
        Check.Fails(() => item.MoveTo(VisitorBatchStatus.New), "move");
    }

    public static void RunAll()
    {
        VisitorBatchStartsInNew();
        VisitorBatchMovesToDone();
        VisitorBatchRejectsMovesFromFailed();
    }
}
