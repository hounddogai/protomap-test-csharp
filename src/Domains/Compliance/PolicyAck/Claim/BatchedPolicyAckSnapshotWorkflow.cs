namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched policy ack snapshots through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedPolicyAckSnapshotManager(IBatchedPolicyAckSnapshotStore Store, BatchedPolicyAckSnapshotValidator Validator, IClock Clock, List<string> Journal, BatchedPolicyAckSnapshotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched policy ack snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedPolicyAckSnapshotStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched policy ack snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedPolicyAckSnapshotStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched policy ack snapshot {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}

/// <summary>Functions of batched policy ack snapshot workflow.</summary>
public static class BatchedPolicyAckSnapshotWorkflow
{
    public static BatchedPolicyAckSnapshotManager NewBatchedPolicyAckSnapshotManager(IBatchedPolicyAckSnapshotStore store, IClock clock)
    {
        return new BatchedPolicyAckSnapshotManager(
            store,
            BatchedPolicyAckSnapshotRules.DefaultBatchedPolicyAckSnapshotValidator(),
            clock,
            new List<string>(),
            BatchedPolicyAckSnapshotEngine.DefaultBatchedPolicyAckSnapshotEngine());
    }
}
