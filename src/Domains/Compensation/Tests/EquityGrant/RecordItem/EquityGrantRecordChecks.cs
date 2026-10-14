namespace Acme.Workforce.Compensation.Tests.EquityGrant.RecordItem;

using Acme.Workforce.Compensation.EquityGrant.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class EquityGrantRecordChecks
{
    public static void EquityGrantRecordStartsInProposed()
    {
        var item = EquityGrantRecord.NewEquityGrantRecord("code-424", "label-934");
        Check.Equal(item.Status, EquityGrantRecordStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantRecordMovesToRejected()
    {
        var item = EquityGrantRecord.NewEquityGrantRecord("code-424", "label-934");
        try
        {
            item.MoveTo(EquityGrantRecordStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantRecordStatus.Rejected, "status");
    }

    public static void EquityGrantRecordRejectsMovesFromWithdrawn()
    {
        var item = EquityGrantRecord.NewEquityGrantRecord("code-424", "label-934");
        item.Status = EquityGrantRecordStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(EquityGrantRecordStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        EquityGrantRecordStartsInProposed();
        EquityGrantRecordMovesToRejected();
        EquityGrantRecordRejectsMovesFromWithdrawn();
    }
}
