namespace Acme.Workforce.Workplace.FloorPlan.Item;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual floor plan items through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualFloorPlanItemCoordinator(IManualFloorPlanItemStore Store, ManualFloorPlanItemValidator Validator, IClock Clock, List<string> Journal, ManualFloorPlanItemEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual floor plan item {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualFloorPlanItemStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual floor plan item {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualFloorPlanItemStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual floor plan item {code} not found");
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

/// <summary>Functions of manual floor plan item workflow.</summary>
public static class ManualFloorPlanItemWorkflow
{
    public static ManualFloorPlanItemCoordinator NewManualFloorPlanItemCoordinator(IManualFloorPlanItemStore store, IClock clock, Registry flags)
    {
        return new ManualFloorPlanItemCoordinator(
            store,
            ManualFloorPlanItemRules.DefaultManualFloorPlanItemValidator(),
            clock,
            new List<string>(),
            ManualFloorPlanItemEngine.DefaultManualFloorPlanItemEngine(flags));
    }
}
