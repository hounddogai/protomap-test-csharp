namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAudit;

using Acme.Workforce.Payrollops.PayRun.RunAudit;

/// <summary>Checks of run audit code, run by the project's runner.</summary>
public static class SeasonalPayRunBatchChecks
{
    public static void SeasonalPayRunBatchStartsInProposed()
    {
        var item = SeasonalPayRunBatch.NewSeasonalPayRunBatch("code-669", "label-235");
        Check.Equal(item.Status, SeasonalPayRunBatchStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalPayRunBatchMovesToRejected()
    {
        var item = SeasonalPayRunBatch.NewSeasonalPayRunBatch("code-669", "label-235");
        try
        {
            item.MoveTo(SeasonalPayRunBatchStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalPayRunBatchStatus.Rejected, "status");
    }

    public static void SeasonalPayRunBatchRejectsMovesFromWithdrawn()
    {
        var item = SeasonalPayRunBatch.NewSeasonalPayRunBatch("code-669", "label-235");
        item.Status = SeasonalPayRunBatchStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SeasonalPayRunBatchStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SeasonalPayRunBatchStartsInProposed();
        SeasonalPayRunBatchMovesToRejected();
        SeasonalPayRunBatchRejectsMovesFromWithdrawn();
    }
}
