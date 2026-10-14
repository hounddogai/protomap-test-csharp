namespace Acme.Workforce.Workplace.Tests.Desk.Variance;

using Acme.Workforce.Workplace.Desk.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class DeskEstimateChecks
{
    public static void DeskEstimateStartsInDraft()
    {
        var item = DeskEstimate.NewDeskEstimate("code-389", "title-898");
        Check.Equal(item.Status, DeskEstimateStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskEstimateMovesToPending()
    {
        var item = DeskEstimate.NewDeskEstimate("code-389", "title-898");
        try
        {
            item.MoveTo(DeskEstimateStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskEstimateStatus.Pending, "status");
    }

    public static void DeskEstimateRejectsMovesFromClosed()
    {
        var item = DeskEstimate.NewDeskEstimate("code-389", "title-898");
        item.Status = DeskEstimateStatus.Closed;
        Check.Fails(() => item.MoveTo(DeskEstimateStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DeskEstimateStartsInDraft();
        DeskEstimateMovesToPending();
        DeskEstimateRejectsMovesFromClosed();
    }
}
