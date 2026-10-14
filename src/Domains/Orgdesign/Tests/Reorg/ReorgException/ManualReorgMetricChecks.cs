namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class ManualReorgMetricChecks
{
    public static void ManualReorgMetricStartsInPlanned()
    {
        var item = ManualReorgMetric.NewManualReorgMetric("code-963", "title-168");
        Check.Equal(item.Status, ManualReorgMetricStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualReorgMetricMovesToInProgress()
    {
        var item = ManualReorgMetric.NewManualReorgMetric("code-963", "title-168");
        try
        {
            item.MoveTo(ManualReorgMetricStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualReorgMetricStatus.InProgress, "status");
    }

    public static void ManualReorgMetricRejectsMovesFromCancelled()
    {
        var item = ManualReorgMetric.NewManualReorgMetric("code-963", "title-168");
        item.Status = ManualReorgMetricStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ManualReorgMetricStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ManualReorgMetricStartsInPlanned();
        ManualReorgMetricMovesToInProgress();
        ManualReorgMetricRejectsMovesFromCancelled();
    }
}
