namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched assignment entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedAssignmentEntryManager(IBatchedAssignmentEntryStore Store, BatchedAssignmentEntryValidator Validator, IClock Clock, List<string> Journal, BatchedAssignmentEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched assignment entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedAssignmentEntryStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched assignment entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedAssignmentEntryStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched assignment entry {code} not found");
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

/// <summary>Functions of batched assignment entry workflow.</summary>
public static class BatchedAssignmentEntryWorkflow
{
    public static BatchedAssignmentEntryManager NewBatchedAssignmentEntryManager(IBatchedAssignmentEntryStore store, IClock clock, Registry flags)
    {
        return new BatchedAssignmentEntryManager(
            store,
            BatchedAssignmentEntryRules.DefaultBatchedAssignmentEntryValidator(),
            clock,
            new List<string>(),
            BatchedAssignmentEntryEngine.DefaultBatchedAssignmentEntryEngine(flags));
    }
}
