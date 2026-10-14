namespace Acme.Workforce.Orgdesign.Tests.Span.Batch;

using Acme.Workforce.Orgdesign.Span.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ManualSpanBatchChecks
{
    public static void ManualSpanBatchStartsInNew()
    {
        var item = ManualSpanBatch.NewManualSpanBatch("code-386", "title-448");
        Check.Equal(item.Status, ManualSpanBatchStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualSpanBatchMovesToRunning()
    {
        var item = ManualSpanBatch.NewManualSpanBatch("code-386", "title-448");
        try
        {
            item.MoveTo(ManualSpanBatchStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualSpanBatchStatus.Running, "status");
    }

    public static void ManualSpanBatchRejectsMovesFromFailed()
    {
        var item = ManualSpanBatch.NewManualSpanBatch("code-386", "title-448");
        item.Status = ManualSpanBatchStatus.Failed;
        Check.Fails(() => item.MoveTo(ManualSpanBatchStatus.New), "move");
    }

    public static void RunAll()
    {
        ManualSpanBatchStartsInNew();
        ManualSpanBatchMovesToRunning();
        ManualSpanBatchRejectsMovesFromFailed();
    }
}
