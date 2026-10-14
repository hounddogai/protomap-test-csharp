namespace Acme.Workforce.Workplace.FloorPlan.Item;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves floor plan snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record FloorPlanSnapshotWorkflow(IFloorPlanSnapshotStore Store, FloorPlanSnapshotValidator Validator, IClock Clock, List<string> Journal)
{
    public static FloorPlanSnapshotWorkflow NewFloorPlanSnapshotWorkflow(IFloorPlanSnapshotStore store, IClock clock)
    {
        return new FloorPlanSnapshotWorkflow(
            store,
            FloorPlanSnapshotRules.DefaultFloorPlanSnapshotValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FloorPlanSnapshotStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FloorPlanSnapshotStatus.Archived);
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
