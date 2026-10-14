namespace Acme.Workforce.Workplace.Occupancy.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves occupancy adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record OccupancyAdjustmentDesk(IOccupancyAdjustmentStore Store, OccupancyAdjustmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OccupancyAdjustmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OccupancyAdjustmentStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of occupancy adjustment workflow.</summary>
public static class OccupancyAdjustmentWorkflow
{
    public static OccupancyAdjustmentDesk NewOccupancyAdjustmentDesk(IOccupancyAdjustmentStore store, IClock clock)
    {
        return new OccupancyAdjustmentDesk(
            store,
            OccupancyAdjustmentRules.DefaultOccupancyAdjustmentValidator(),
            clock,
            new List<string>());
    }
}
