namespace Acme.Workforce.Compensation.Tests.MeritCycle.Batch;

using Acme.Workforce.Compensation.MeritCycle.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PendingMeritCycleBatchChecks
{
    public static void PendingMeritCycleBatchStartsInRequested()
    {
        var item = PendingMeritCycleBatch.NewPendingMeritCycleBatch("code-524", "title-208");
        Check.Equal(item.Status, PendingMeritCycleBatchStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingMeritCycleBatchMovesToExpired()
    {
        var item = PendingMeritCycleBatch.NewPendingMeritCycleBatch("code-524", "title-208");
        try
        {
            item.MoveTo(PendingMeritCycleBatchStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingMeritCycleBatchStatus.Expired, "status");
    }

    public static void PendingMeritCycleBatchRejectsMovesFromExpired()
    {
        var item = PendingMeritCycleBatch.NewPendingMeritCycleBatch("code-524", "title-208");
        item.Status = PendingMeritCycleBatchStatus.Expired;
        Check.Fails(() => item.MoveTo(PendingMeritCycleBatchStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PendingMeritCycleBatchStartsInRequested();
        PendingMeritCycleBatchMovesToExpired();
        PendingMeritCycleBatchRejectsMovesFromExpired();
    }
}
