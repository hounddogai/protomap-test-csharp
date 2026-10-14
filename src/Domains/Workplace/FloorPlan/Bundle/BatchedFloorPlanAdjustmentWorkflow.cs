namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched floor plan adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedFloorPlanAdjustmentCoordinator(IBatchedFloorPlanAdjustmentStore Store, BatchedFloorPlanAdjustmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched floor plan adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedFloorPlanAdjustmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched floor plan adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedFloorPlanAdjustmentStatus.Stopped);
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

/// <summary>Functions of batched floor plan adjustment workflow.</summary>
public static class BatchedFloorPlanAdjustmentWorkflow
{
    public static BatchedFloorPlanAdjustmentCoordinator NewBatchedFloorPlanAdjustmentCoordinator(IBatchedFloorPlanAdjustmentStore store, IClock clock)
    {
        return new BatchedFloorPlanAdjustmentCoordinator(
            store,
            BatchedFloorPlanAdjustmentRules.DefaultBatchedFloorPlanAdjustmentValidator(),
            clock,
            new List<string>());
    }
}
