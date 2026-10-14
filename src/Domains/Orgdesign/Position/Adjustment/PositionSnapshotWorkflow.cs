namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves position snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionSnapshotCoordinator(IPositionSnapshotStore Store, PositionSnapshotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionSnapshotStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionSnapshotStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of position snapshot workflow.</summary>
public static class PositionSnapshotWorkflow
{
    public static PositionSnapshotCoordinator NewPositionSnapshotCoordinator(IPositionSnapshotStore store, IClock clock)
    {
        return new PositionSnapshotCoordinator(
            store,
            PositionSnapshotRules.DefaultPositionSnapshotValidator(),
            clock,
            new List<string>());
    }
}
