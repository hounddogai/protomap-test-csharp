namespace Acme.Workforce.Orgdesign.Tests.Span.SpanReview;

using Acme.Workforce.Orgdesign.Span.SpanReview;

/// <summary>Checks of span review code, run by the project's runner.</summary>
public static class GlobalSpanAdjustmentChecks
{
    public static void GlobalSpanAdjustmentStartsInPlanned()
    {
        var item = GlobalSpanAdjustment.NewGlobalSpanAdjustment("code-709", "title-59");
        Check.Equal(item.Status, GlobalSpanAdjustmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalSpanAdjustmentMovesToInProgress()
    {
        var item = GlobalSpanAdjustment.NewGlobalSpanAdjustment("code-709", "title-59");
        try
        {
            item.MoveTo(GlobalSpanAdjustmentStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalSpanAdjustmentStatus.InProgress, "status");
    }

    public static void GlobalSpanAdjustmentRejectsMovesFromCancelled()
    {
        var item = GlobalSpanAdjustment.NewGlobalSpanAdjustment("code-709", "title-59");
        item.Status = GlobalSpanAdjustmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(GlobalSpanAdjustmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        GlobalSpanAdjustmentStartsInPlanned();
        GlobalSpanAdjustmentMovesToInProgress();
        GlobalSpanAdjustmentRejectsMovesFromCancelled();
    }
}
