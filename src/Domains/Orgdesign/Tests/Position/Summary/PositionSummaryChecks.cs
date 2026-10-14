namespace Acme.Workforce.Orgdesign.Tests.Position.Summary;

using Acme.Workforce.Orgdesign.Position.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PositionSummaryChecks
{
    public static void PositionSummaryStartsInDraft()
    {
        var item = PositionSummary.NewPositionSummary("code-92", "title-447");
        Check.Equal(item.Status, PositionSummaryStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionSummaryMovesToActive()
    {
        var item = PositionSummary.NewPositionSummary("code-92", "title-447");
        try
        {
            item.MoveTo(PositionSummaryStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionSummaryStatus.Active, "status");
    }

    public static void PositionSummaryRejectsMovesFromClosed()
    {
        var item = PositionSummary.NewPositionSummary("code-92", "title-447");
        item.Status = PositionSummaryStatus.Closed;
        Check.Fails(() => item.MoveTo(PositionSummaryStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PositionSummaryStartsInDraft();
        PositionSummaryMovesToActive();
        PositionSummaryRejectsMovesFromClosed();
    }
}
