namespace Acme.Workforce.Engagement.Tests.TownHall.Ledger;

using Acme.Workforce.Engagement.TownHall.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class TownHallSummaryChecks
{
    public static void TownHallSummaryStartsInNew()
    {
        var item = TownHallSummary.NewTownHallSummary("code-245", "label-157");
        Check.Equal(item.Status, TownHallSummaryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallSummaryMovesToQueued()
    {
        var item = TownHallSummary.NewTownHallSummary("code-245", "label-157");
        try
        {
            item.MoveTo(TownHallSummaryStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallSummaryStatus.Queued, "status");
    }

    public static void TownHallSummaryRejectsMovesFromFailed()
    {
        var item = TownHallSummary.NewTownHallSummary("code-245", "label-157");
        item.Status = TownHallSummaryStatus.Failed;
        Check.Fails(() => item.MoveTo(TownHallSummaryStatus.New), "move");
    }

    public static void RunAll()
    {
        TownHallSummaryStartsInNew();
        TownHallSummaryMovesToQueued();
        TownHallSummaryRejectsMovesFromFailed();
    }
}
