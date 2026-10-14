namespace Acme.Workforce.Workplace.Tests.Desk.DeskAllocation;

using Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>Checks of desk allocation code, run by the project's runner.</summary>
public static class TieredDeskMetricChecks
{
    public static void TieredDeskMetricStartsInPlanned()
    {
        var item = TieredDeskMetric.NewTieredDeskMetric("code-127", "name-576");
        Check.Equal(item.Status, TieredDeskMetricStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredDeskMetricMovesToInProgress()
    {
        var item = TieredDeskMetric.NewTieredDeskMetric("code-127", "name-576");
        try
        {
            item.MoveTo(TieredDeskMetricStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredDeskMetricStatus.InProgress, "status");
    }

    public static void TieredDeskMetricRejectsMovesFromCancelled()
    {
        var item = TieredDeskMetric.NewTieredDeskMetric("code-127", "name-576");
        item.Status = TieredDeskMetricStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TieredDeskMetricStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TieredDeskMetricStartsInPlanned();
        TieredDeskMetricMovesToInProgress();
        TieredDeskMetricRejectsMovesFromCancelled();
    }
}
