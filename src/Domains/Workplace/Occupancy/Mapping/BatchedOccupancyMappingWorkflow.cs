namespace Acme.Workforce.Workplace.Occupancy.Mapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched occupancy mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedOccupancyMappingCoordinator(IBatchedOccupancyMappingStore Store, BatchedOccupancyMappingValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched occupancy mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedOccupancyMappingStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched occupancy mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedOccupancyMappingStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of batched occupancy mapping workflow.</summary>
public static class BatchedOccupancyMappingWorkflow
{
    public static BatchedOccupancyMappingCoordinator NewBatchedOccupancyMappingCoordinator(IBatchedOccupancyMappingStore store, IClock clock)
    {
        return new BatchedOccupancyMappingCoordinator(
            store,
            BatchedOccupancyMappingRules.DefaultBatchedOccupancyMappingValidator(),
            clock,
            new List<string>());
    }
}
