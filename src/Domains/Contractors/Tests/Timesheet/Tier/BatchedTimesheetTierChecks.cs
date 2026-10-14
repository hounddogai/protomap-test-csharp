namespace Acme.Workforce.Contractors.Tests.Timesheet.Tier;

using Acme.Workforce.Contractors.Timesheet.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BatchedTimesheetTierChecks
{
    public static void BatchedTimesheetTierStartsInPlanned()
    {
        var item = BatchedTimesheetTier.NewBatchedTimesheetTier("code-635", "label-204");
        Check.Equal(item.Status, BatchedTimesheetTierStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedTimesheetTierMovesToScheduled()
    {
        var item = BatchedTimesheetTier.NewBatchedTimesheetTier("code-635", "label-204");
        try
        {
            item.MoveTo(BatchedTimesheetTierStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedTimesheetTierStatus.Scheduled, "status");
    }

    public static void BatchedTimesheetTierRejectsMovesFromCancelled()
    {
        var item = BatchedTimesheetTier.NewBatchedTimesheetTier("code-635", "label-204");
        item.Status = BatchedTimesheetTierStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BatchedTimesheetTierStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BatchedTimesheetTierStartsInPlanned();
        BatchedTimesheetTierMovesToScheduled();
        BatchedTimesheetTierRejectsMovesFromCancelled();
    }
}
