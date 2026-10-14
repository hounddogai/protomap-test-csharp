namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgClaim;

using Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

/// <summary>Checks of reorg claim code, run by the project's runner.</summary>
public static class RollingReorgSummaryChecks
{
    public static void RollingReorgSummaryStartsInNew()
    {
        var item = RollingReorgSummary.NewRollingReorgSummary("code-752", "name-175");
        Check.Equal(item.Status, RollingReorgSummaryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingReorgSummaryMovesToQueued()
    {
        var item = RollingReorgSummary.NewRollingReorgSummary("code-752", "name-175");
        try
        {
            item.MoveTo(RollingReorgSummaryStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingReorgSummaryStatus.Queued, "status");
    }

    public static void RollingReorgSummaryRejectsMovesFromFailed()
    {
        var item = RollingReorgSummary.NewRollingReorgSummary("code-752", "name-175");
        item.Status = RollingReorgSummaryStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingReorgSummaryStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingReorgSummaryStartsInNew();
        RollingReorgSummaryMovesToQueued();
        RollingReorgSummaryRejectsMovesFromFailed();
    }
}
