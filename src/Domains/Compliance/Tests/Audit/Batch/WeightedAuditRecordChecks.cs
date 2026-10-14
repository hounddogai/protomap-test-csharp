namespace Acme.Workforce.Compliance.Tests.Audit.Batch;

using Acme.Workforce.Compliance.Audit.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class WeightedAuditRecordChecks
{
    public static void WeightedAuditRecordStartsInDraft()
    {
        var item = WeightedAuditRecord.NewWeightedAuditRecord("code-249", "label-597");
        Check.Equal(item.Status, WeightedAuditRecordStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedAuditRecordMovesToPending()
    {
        var item = WeightedAuditRecord.NewWeightedAuditRecord("code-249", "label-597");
        try
        {
            item.MoveTo(WeightedAuditRecordStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedAuditRecordStatus.Pending, "status");
    }

    public static void WeightedAuditRecordRejectsMovesFromClosed()
    {
        var item = WeightedAuditRecord.NewWeightedAuditRecord("code-249", "label-597");
        item.Status = WeightedAuditRecordStatus.Closed;
        Check.Fails(() => item.MoveTo(WeightedAuditRecordStatus.Draft), "move");
    }

    public static void RunAll()
    {
        WeightedAuditRecordStartsInDraft();
        WeightedAuditRecordMovesToPending();
        WeightedAuditRecordRejectsMovesFromClosed();
    }
}
