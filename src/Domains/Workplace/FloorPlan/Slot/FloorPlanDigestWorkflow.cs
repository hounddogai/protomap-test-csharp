namespace Acme.Workforce.Workplace.FloorPlan.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves floor plan digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record FloorPlanDigestService(IFloorPlanDigestStore Store, FloorPlanDigestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FloorPlanDigestStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FloorPlanDigestStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of floor plan digest workflow.</summary>
public static class FloorPlanDigestWorkflow
{
    public static FloorPlanDigestService NewFloorPlanDigestService(IFloorPlanDigestStore store, IClock clock)
    {
        return new FloorPlanDigestService(
            store,
            FloorPlanDigestRules.DefaultFloorPlanDigestValidator(),
            clock,
            new List<string>());
    }
}
