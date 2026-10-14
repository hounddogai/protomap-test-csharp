namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSummary;

using Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

/// <summary>Checks of reorg summary code, run by the project's runner.</summary>
public static class LegacyReorgSummaryChecks
{
    public static void LegacyReorgSummaryStartsInPlanned()
    {
        var item = LegacyReorgSummary.NewLegacyReorgSummary("code-711", "title-16");
        Check.Equal(item.Status, LegacyReorgSummaryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyReorgSummaryMovesToCancelled()
    {
        var item = LegacyReorgSummary.NewLegacyReorgSummary("code-711", "title-16");
        try
        {
            item.MoveTo(LegacyReorgSummaryStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyReorgSummaryStatus.Cancelled, "status");
    }

    public static void LegacyReorgSummaryRejectsMovesFromCancelled()
    {
        var item = LegacyReorgSummary.NewLegacyReorgSummary("code-711", "title-16");
        item.Status = LegacyReorgSummaryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LegacyReorgSummaryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LegacyReorgSummaryStartsInPlanned();
        LegacyReorgSummaryMovesToCancelled();
        LegacyReorgSummaryRejectsMovesFromCancelled();
    }
}
