namespace Acme.Workforce.Engagement.TownHall.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched town hall batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedTownHallBatchManager(IBatchedTownHallBatchStore Store, BatchedTownHallBatchValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched town hall batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedTownHallBatchStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched town hall batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedTownHallBatchStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of batched town hall batch workflow.</summary>
public static class BatchedTownHallBatchWorkflow
{
    public static BatchedTownHallBatchManager NewBatchedTownHallBatchManager(IBatchedTownHallBatchStore store, IClock clock)
    {
        return new BatchedTownHallBatchManager(
            store,
            BatchedTownHallBatchRules.DefaultBatchedTownHallBatchValidator(),
            clock,
            new List<string>());
    }
}
