namespace Acme.Workforce.Orgdesign.Tests.Reorg.Summary;

using Acme.Workforce.Orgdesign.Reorg.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ReorgSummaryChecks
{
    public static void ReorgSummaryStartsInNew()
    {
        var item = ReorgSummary.NewReorgSummary("code-647", "label-612");
        Check.Equal(item.Status, ReorgSummaryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgSummaryMovesToFailed()
    {
        var item = ReorgSummary.NewReorgSummary("code-647", "label-612");
        try
        {
            item.MoveTo(ReorgSummaryStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgSummaryStatus.Failed, "status");
    }

    public static void ReorgSummaryRejectsMovesFromFailed()
    {
        var item = ReorgSummary.NewReorgSummary("code-647", "label-612");
        item.Status = ReorgSummaryStatus.Failed;
        Check.Fails(() => item.MoveTo(ReorgSummaryStatus.New), "move");
    }

    public static void RunAll()
    {
        ReorgSummaryStartsInNew();
        ReorgSummaryMovesToFailed();
        ReorgSummaryRejectsMovesFromFailed();
    }
}
