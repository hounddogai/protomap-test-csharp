namespace Acme.Workforce.Talent.Pool.Milestone;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global pool snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalPoolSnapshotCoordinator(IGlobalPoolSnapshotStore Store, GlobalPoolSnapshotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global pool snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalPoolSnapshotStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global pool snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalPoolSnapshotStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of global pool snapshot workflow.</summary>
public static class GlobalPoolSnapshotWorkflow
{
    public static GlobalPoolSnapshotCoordinator NewGlobalPoolSnapshotCoordinator(IGlobalPoolSnapshotStore store, IClock clock)
    {
        return new GlobalPoolSnapshotCoordinator(
            store,
            GlobalPoolSnapshotRules.DefaultGlobalPoolSnapshotValidator(),
            clock,
            new List<string>());
    }
}
