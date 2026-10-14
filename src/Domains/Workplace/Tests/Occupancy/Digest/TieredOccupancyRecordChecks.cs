namespace Acme.Workforce.Workplace.Tests.Occupancy.Digest;

using Acme.Workforce.Workplace.Occupancy.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class TieredOccupancyRecordChecks
{
    public static void TieredOccupancyRecordStartsInPlanned()
    {
        var item = TieredOccupancyRecord.NewTieredOccupancyRecord("code-596", "label-939");
        Check.Equal(item.Status, TieredOccupancyRecordStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredOccupancyRecordMovesToInProgress()
    {
        var item = TieredOccupancyRecord.NewTieredOccupancyRecord("code-596", "label-939");
        try
        {
            item.MoveTo(TieredOccupancyRecordStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredOccupancyRecordStatus.InProgress, "status");
    }

    public static void TieredOccupancyRecordRejectsMovesFromCancelled()
    {
        var item = TieredOccupancyRecord.NewTieredOccupancyRecord("code-596", "label-939");
        item.Status = TieredOccupancyRecordStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TieredOccupancyRecordStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TieredOccupancyRecordStartsInPlanned();
        TieredOccupancyRecordMovesToInProgress();
        TieredOccupancyRecordRejectsMovesFromCancelled();
    }
}
