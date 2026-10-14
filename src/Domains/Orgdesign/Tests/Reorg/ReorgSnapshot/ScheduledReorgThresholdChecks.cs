namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSnapshot;

using Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

/// <summary>Checks of reorg snapshot code, run by the project's runner.</summary>
public static class ScheduledReorgThresholdChecks
{
    public static void ScheduledReorgThresholdStartsInOpen()
    {
        var item = ScheduledReorgThreshold.NewScheduledReorgThreshold("code-175", "label-482");
        Check.Equal(item.Status, ScheduledReorgThresholdStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledReorgThresholdMovesToResolved()
    {
        var item = ScheduledReorgThreshold.NewScheduledReorgThreshold("code-175", "label-482");
        try
        {
            item.MoveTo(ScheduledReorgThresholdStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledReorgThresholdStatus.Resolved, "status");
    }

    public static void ScheduledReorgThresholdRejectsMovesFromArchived()
    {
        var item = ScheduledReorgThreshold.NewScheduledReorgThreshold("code-175", "label-482");
        item.Status = ScheduledReorgThresholdStatus.Archived;
        Check.Fails(() => item.MoveTo(ScheduledReorgThresholdStatus.Open), "move");
    }

    public static void RunAll()
    {
        ScheduledReorgThresholdStartsInOpen();
        ScheduledReorgThresholdMovesToResolved();
        ScheduledReorgThresholdRejectsMovesFromArchived();
    }
}
