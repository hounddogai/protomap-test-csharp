namespace Acme.Workforce.Absence.Tests.LeaveRequest.Forecast;

using Acme.Workforce.Absence.LeaveRequest.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class BatchedLeaveRequestForecastChecks
{
    public static void BatchedLeaveRequestForecastStartsInProposed()
    {
        var item = BatchedLeaveRequestForecast.NewBatchedLeaveRequestForecast("code-971", "name-691");
        Check.Equal(item.Status, BatchedLeaveRequestForecastStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedLeaveRequestForecastMovesToWithdrawn()
    {
        var item = BatchedLeaveRequestForecast.NewBatchedLeaveRequestForecast("code-971", "name-691");
        try
        {
            item.MoveTo(BatchedLeaveRequestForecastStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedLeaveRequestForecastStatus.Withdrawn, "status");
    }

    public static void BatchedLeaveRequestForecastRejectsMovesFromWithdrawn()
    {
        var item = BatchedLeaveRequestForecast.NewBatchedLeaveRequestForecast("code-971", "name-691");
        item.Status = BatchedLeaveRequestForecastStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BatchedLeaveRequestForecastStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BatchedLeaveRequestForecastStartsInProposed();
        BatchedLeaveRequestForecastMovesToWithdrawn();
        BatchedLeaveRequestForecastRejectsMovesFromWithdrawn();
    }
}
