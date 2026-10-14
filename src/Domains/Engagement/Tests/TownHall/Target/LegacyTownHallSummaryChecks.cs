namespace Acme.Workforce.Engagement.Tests.TownHall.Target;

using Acme.Workforce.Engagement.TownHall.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LegacyTownHallSummaryChecks
{
    public static void LegacyTownHallSummaryStartsInOpen()
    {
        var item = LegacyTownHallSummary.NewLegacyTownHallSummary("code-633", "label-517");
        Check.Equal(item.Status, LegacyTownHallSummaryStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyTownHallSummaryMovesToInReview()
    {
        var item = LegacyTownHallSummary.NewLegacyTownHallSummary("code-633", "label-517");
        try
        {
            item.MoveTo(LegacyTownHallSummaryStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyTownHallSummaryStatus.InReview, "status");
    }

    public static void LegacyTownHallSummaryRejectsMovesFromArchived()
    {
        var item = LegacyTownHallSummary.NewLegacyTownHallSummary("code-633", "label-517");
        item.Status = LegacyTownHallSummaryStatus.Archived;
        Check.Fails(() => item.MoveTo(LegacyTownHallSummaryStatus.Open), "move");
    }

    public static void RunAll()
    {
        LegacyTownHallSummaryStartsInOpen();
        LegacyTownHallSummaryMovesToInReview();
        LegacyTownHallSummaryRejectsMovesFromArchived();
    }
}
