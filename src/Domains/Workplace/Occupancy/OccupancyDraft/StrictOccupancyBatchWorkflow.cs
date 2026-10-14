namespace Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict occupancy batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictOccupancyBatchService(IStrictOccupancyBatchStore Store, StrictOccupancyBatchValidator Validator, IClock Clock, List<string> Journal, StrictOccupancyBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict occupancy batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictOccupancyBatchStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict occupancy batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictOccupancyBatchStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict occupancy batch {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of strict occupancy batch workflow.</summary>
public static class StrictOccupancyBatchWorkflow
{
    public static StrictOccupancyBatchService NewStrictOccupancyBatchService(IStrictOccupancyBatchStore store, IClock clock)
    {
        return new StrictOccupancyBatchService(
            store,
            StrictOccupancyBatchRules.DefaultStrictOccupancyBatchValidator(),
            clock,
            new List<string>(),
            StrictOccupancyBatchEngine.DefaultStrictOccupancyBatchEngine());
    }
}
