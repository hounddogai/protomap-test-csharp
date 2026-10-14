namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered assignment snapshots through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredAssignmentSnapshotDesk(ITieredAssignmentSnapshotStore Store, TieredAssignmentSnapshotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered assignment snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredAssignmentSnapshotStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered assignment snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredAssignmentSnapshotStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of tiered assignment snapshot workflow.</summary>
public static class TieredAssignmentSnapshotWorkflow
{
    public static TieredAssignmentSnapshotDesk NewTieredAssignmentSnapshotDesk(ITieredAssignmentSnapshotStore store, IClock clock)
    {
        return new TieredAssignmentSnapshotDesk(
            store,
            TieredAssignmentSnapshotRules.DefaultTieredAssignmentSnapshotValidator(),
            clock,
            new List<string>());
    }
}
