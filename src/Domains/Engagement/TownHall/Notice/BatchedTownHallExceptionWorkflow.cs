namespace Acme.Workforce.Engagement.TownHall.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched town hall exceptions through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedTownHallExceptionService(IBatchedTownHallExceptionStore Store, BatchedTownHallExceptionValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched town hall exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedTownHallExceptionStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched town hall exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedTownHallExceptionStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of batched town hall exception workflow.</summary>
public static class BatchedTownHallExceptionWorkflow
{
    public static BatchedTownHallExceptionService NewBatchedTownHallExceptionService(IBatchedTownHallExceptionStore store, IClock clock)
    {
        return new BatchedTownHallExceptionService(
            store,
            BatchedTownHallExceptionRules.DefaultBatchedTownHallExceptionValidator(),
            clock,
            new List<string>());
    }
}
