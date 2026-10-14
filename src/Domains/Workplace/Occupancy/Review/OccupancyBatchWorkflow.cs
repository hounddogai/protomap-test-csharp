namespace Acme.Workforce.Workplace.Occupancy.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves occupancy batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record OccupancyBatchService(IOccupancyBatchStore Store, OccupancyBatchValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OccupancyBatchStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OccupancyBatchStatus.Archived);
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of occupancy batch workflow.</summary>
public static class OccupancyBatchWorkflow
{
    public static OccupancyBatchService NewOccupancyBatchService(IOccupancyBatchStore store, IClock clock)
    {
        return new OccupancyBatchService(
            store,
            OccupancyBatchRules.DefaultOccupancyBatchValidator(),
            clock,
            new List<string>());
    }
}
