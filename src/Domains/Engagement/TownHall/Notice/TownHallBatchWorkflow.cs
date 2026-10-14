namespace Acme.Workforce.Engagement.TownHall.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves town hall batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallBatchService(ITownHallBatchStore Store, TownHallBatchValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallBatchStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallBatchStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of town hall batch workflow.</summary>
public static class TownHallBatchWorkflow
{
    public static TownHallBatchService NewTownHallBatchService(ITownHallBatchStore store, IClock clock)
    {
        return new TownHallBatchService(
            store,
            TownHallBatchRules.DefaultTownHallBatchValidator(),
            clock,
            new List<string>());
    }
}
