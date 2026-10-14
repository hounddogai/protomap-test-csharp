namespace Acme.Workforce.Absence.LeaveRequest.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched leave request items through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedLeaveRequestItemService(IBatchedLeaveRequestItemStore Store, BatchedLeaveRequestItemValidator Validator, IClock Clock, List<string> Journal, BatchedLeaveRequestItemEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched leave request item {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedLeaveRequestItemStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched leave request item {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedLeaveRequestItemStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched leave request item {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of batched leave request item workflow.</summary>
public static class BatchedLeaveRequestItemWorkflow
{
    public static BatchedLeaveRequestItemService NewBatchedLeaveRequestItemService(IBatchedLeaveRequestItemStore store, IClock clock)
    {
        return new BatchedLeaveRequestItemService(
            store,
            BatchedLeaveRequestItemRules.DefaultBatchedLeaveRequestItemValidator(),
            clock,
            new List<string>(),
            BatchedLeaveRequestItemEngine.DefaultBatchedLeaveRequestItemEngine());
    }
}
