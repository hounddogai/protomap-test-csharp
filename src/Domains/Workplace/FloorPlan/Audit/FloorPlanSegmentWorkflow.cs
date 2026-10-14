namespace Acme.Workforce.Workplace.FloorPlan.Audit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves floor plan segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record FloorPlanSegmentCoordinator(IFloorPlanSegmentStore Store, FloorPlanSegmentValidator Validator, IClock Clock, List<string> Journal, FloorPlanSegmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FloorPlanSegmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FloorPlanSegmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan segment {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of floor plan segment workflow.</summary>
public static class FloorPlanSegmentWorkflow
{
    public static FloorPlanSegmentCoordinator NewFloorPlanSegmentCoordinator(IFloorPlanSegmentStore store, IClock clock)
    {
        return new FloorPlanSegmentCoordinator(
            store,
            FloorPlanSegmentRules.DefaultFloorPlanSegmentValidator(),
            clock,
            new List<string>(),
            FloorPlanSegmentEngine.DefaultFloorPlanSegmentEngine());
    }
}
