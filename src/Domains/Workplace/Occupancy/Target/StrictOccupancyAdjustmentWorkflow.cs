namespace Acme.Workforce.Workplace.Occupancy.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict occupancy adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StrictOccupancyAdjustmentCoordinator(IStrictOccupancyAdjustmentStore Store, StrictOccupancyAdjustmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict occupancy adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictOccupancyAdjustmentStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict occupancy adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictOccupancyAdjustmentStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of strict occupancy adjustment workflow.</summary>
public static class StrictOccupancyAdjustmentWorkflow
{
    public static StrictOccupancyAdjustmentCoordinator NewStrictOccupancyAdjustmentCoordinator(IStrictOccupancyAdjustmentStore store, IClock clock)
    {
        return new StrictOccupancyAdjustmentCoordinator(
            store,
            StrictOccupancyAdjustmentRules.DefaultStrictOccupancyAdjustmentValidator(),
            clock,
            new List<string>());
    }
}
