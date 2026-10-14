namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskMetricChecks
{
    public static void DeskMetricStartsInOpen()
    {
        var item = DeskMetric.NewDeskMetric("code-31", "name-846");
        Check.Equal(item.Status, DeskMetricStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskMetricMovesToInReview()
    {
        var item = DeskMetric.NewDeskMetric("code-31", "name-846");
        try
        {
            item.MoveTo(DeskMetricStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskMetricStatus.InReview, "status");
    }

    public static void DeskMetricRejectsMovesFromArchived()
    {
        var item = DeskMetric.NewDeskMetric("code-31", "name-846");
        item.Status = DeskMetricStatus.Archived;
        Check.Fails(() => item.MoveTo(DeskMetricStatus.Open), "move");
    }

    public static void RunAll()
    {
        DeskMetricStartsInOpen();
        DeskMetricMovesToInReview();
        DeskMetricRejectsMovesFromArchived();
    }
}
