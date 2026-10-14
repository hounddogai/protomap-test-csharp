namespace Acme.Workforce.Orgdesign.Tests.Span.TieredSpanDraft;

using Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

/// <summary>Checks of tiered span draft code, run by the project's runner.</summary>
public static class SpanBatchChecks
{
    public static void SpanBatchStartsInProposed()
    {
        var item = SpanBatch.NewSpanBatch("code-465", "label-815");
        Check.Equal(item.Status, SpanBatchStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanBatchMovesToApproved()
    {
        var item = SpanBatch.NewSpanBatch("code-465", "label-815");
        try
        {
            item.MoveTo(SpanBatchStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanBatchStatus.Approved, "status");
    }

    public static void SpanBatchRejectsMovesFromWithdrawn()
    {
        var item = SpanBatch.NewSpanBatch("code-465", "label-815");
        item.Status = SpanBatchStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SpanBatchStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SpanBatchStartsInProposed();
        SpanBatchMovesToApproved();
        SpanBatchRejectsMovesFromWithdrawn();
    }
}
