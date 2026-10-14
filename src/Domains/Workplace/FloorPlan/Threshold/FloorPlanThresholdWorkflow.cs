namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves floor plan thresholds through their life cycle and keeps a journal of what it did.</summary>
public sealed record FloorPlanThresholdCoordinator(IFloorPlanThresholdStore Store, FloorPlanThresholdValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FloorPlanThresholdStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FloorPlanThresholdStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public int Scan(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of floor plan threshold workflow.</summary>
public static class FloorPlanThresholdWorkflow
{
    public static FloorPlanThresholdCoordinator NewFloorPlanThresholdCoordinator(IFloorPlanThresholdStore store, IClock clock)
    {
        return new FloorPlanThresholdCoordinator(
            store,
            FloorPlanThresholdRules.DefaultFloorPlanThresholdValidator(),
            clock,
            new List<string>());
    }
}
