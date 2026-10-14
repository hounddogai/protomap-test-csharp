namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adjustment snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdjustmentSnapshotManager(IAdjustmentSnapshotStore Store, AdjustmentSnapshotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdjustmentSnapshotStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdjustmentSnapshotStatus.Archived);
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

/// <summary>Functions of adjustment snapshot workflow.</summary>
public static class AdjustmentSnapshotWorkflow
{
    public static AdjustmentSnapshotManager NewAdjustmentSnapshotManager(IAdjustmentSnapshotStore store, IClock clock)
    {
        return new AdjustmentSnapshotManager(
            store,
            AdjustmentSnapshotRules.DefaultAdjustmentSnapshotValidator(),
            clock,
            new List<string>());
    }
}
