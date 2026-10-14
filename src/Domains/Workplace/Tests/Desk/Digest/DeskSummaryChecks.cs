namespace Acme.Workforce.Workplace.Tests.Desk.Digest;

using Acme.Workforce.Workplace.Desk.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class DeskSummaryChecks
{
    public static void DeskSummaryStartsInIdle()
    {
        var item = DeskSummary.NewDeskSummary("code-652", "label-720");
        Check.Equal(item.Status, DeskSummaryStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskSummaryMovesToReady()
    {
        var item = DeskSummary.NewDeskSummary("code-652", "label-720");
        try
        {
            item.MoveTo(DeskSummaryStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskSummaryStatus.Ready, "status");
    }

    public static void DeskSummaryRejectsMovesFromStopped()
    {
        var item = DeskSummary.NewDeskSummary("code-652", "label-720");
        item.Status = DeskSummaryStatus.Stopped;
        Check.Fails(() => item.MoveTo(DeskSummaryStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DeskSummaryStartsInIdle();
        DeskSummaryMovesToReady();
        DeskSummaryRejectsMovesFromStopped();
    }
}
