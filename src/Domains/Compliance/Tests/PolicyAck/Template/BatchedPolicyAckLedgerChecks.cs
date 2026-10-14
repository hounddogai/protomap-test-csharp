namespace Acme.Workforce.Compliance.Tests.PolicyAck.Template;

using Acme.Workforce.Compliance.PolicyAck.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class BatchedPolicyAckLedgerChecks
{
    public static void BatchedPolicyAckLedgerStartsInProposed()
    {
        var item = BatchedPolicyAckLedger.NewBatchedPolicyAckLedger("code-872", "title-892");
        Check.Equal(item.Status, BatchedPolicyAckLedgerStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedPolicyAckLedgerMovesToRejected()
    {
        var item = BatchedPolicyAckLedger.NewBatchedPolicyAckLedger("code-872", "title-892");
        try
        {
            item.MoveTo(BatchedPolicyAckLedgerStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedPolicyAckLedgerStatus.Rejected, "status");
    }

    public static void BatchedPolicyAckLedgerRejectsMovesFromWithdrawn()
    {
        var item = BatchedPolicyAckLedger.NewBatchedPolicyAckLedger("code-872", "title-892");
        item.Status = BatchedPolicyAckLedgerStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BatchedPolicyAckLedgerStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BatchedPolicyAckLedgerStartsInProposed();
        BatchedPolicyAckLedgerMovesToRejected();
        BatchedPolicyAckLedgerRejectsMovesFromWithdrawn();
    }
}
