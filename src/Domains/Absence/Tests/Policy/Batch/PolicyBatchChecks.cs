namespace Acme.Workforce.Absence.Tests.Policy.Batch;

using Acme.Workforce.Absence.Policy.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PolicyBatchChecks
{
    public static void PolicyBatchStartsInProposed()
    {
        var item = PolicyBatch.NewPolicyBatch("code-768", "label-818");
        Check.Equal(item.Status, PolicyBatchStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyBatchMovesToRejected()
    {
        var item = PolicyBatch.NewPolicyBatch("code-768", "label-818");
        try
        {
            item.MoveTo(PolicyBatchStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyBatchStatus.Rejected, "status");
    }

    public static void PolicyBatchRejectsMovesFromWithdrawn()
    {
        var item = PolicyBatch.NewPolicyBatch("code-768", "label-818");
        item.Status = PolicyBatchStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PolicyBatchStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PolicyBatchStartsInProposed();
        PolicyBatchMovesToRejected();
        PolicyBatchRejectsMovesFromWithdrawn();
    }
}
