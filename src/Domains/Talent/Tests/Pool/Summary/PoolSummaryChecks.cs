namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PoolSummaryChecks
{
    public static void PoolSummaryStartsInPlanned()
    {
        var item = PoolSummary.NewPoolSummary("code-959", "name-632");
        Check.Equal(item.Status, PoolSummaryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolSummaryMovesToInProgress()
    {
        var item = PoolSummary.NewPoolSummary("code-959", "name-632");
        try
        {
            item.MoveTo(PoolSummaryStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolSummaryStatus.InProgress, "status");
    }

    public static void PoolSummaryRejectsMovesFromCancelled()
    {
        var item = PoolSummary.NewPoolSummary("code-959", "name-632");
        item.Status = PoolSummaryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PoolSummaryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PoolSummaryStartsInPlanned();
        PoolSummaryMovesToInProgress();
        PoolSummaryRejectsMovesFromCancelled();
    }
}
