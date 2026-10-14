namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled floor plan snapshots through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledFloorPlanSnapshotDesk(IScheduledFloorPlanSnapshotStore Store, ScheduledFloorPlanSnapshotValidator Validator, IClock Clock, List<string> Journal, ScheduledFloorPlanSnapshotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled floor plan snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledFloorPlanSnapshotStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled floor plan snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledFloorPlanSnapshotStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled floor plan snapshot {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of scheduled floor plan snapshot workflow.</summary>
public static class ScheduledFloorPlanSnapshotWorkflow
{
    public static ScheduledFloorPlanSnapshotDesk NewScheduledFloorPlanSnapshotDesk(IScheduledFloorPlanSnapshotStore store, IClock clock, Registry flags)
    {
        return new ScheduledFloorPlanSnapshotDesk(
            store,
            ScheduledFloorPlanSnapshotRules.DefaultScheduledFloorPlanSnapshotValidator(),
            clock,
            new List<string>(),
            ScheduledFloorPlanSnapshotEngine.DefaultScheduledFloorPlanSnapshotEngine(flags));
    }
}
