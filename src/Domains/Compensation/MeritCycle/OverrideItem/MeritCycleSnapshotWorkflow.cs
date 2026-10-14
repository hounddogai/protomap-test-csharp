namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves merit cycle snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleSnapshotCoordinator(IMeritCycleSnapshotStore Store, MeritCycleSnapshotValidator Validator, IClock Clock, List<string> Journal, MeritCycleSnapshotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleSnapshotStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleSnapshotStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle snapshot {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of merit cycle snapshot workflow.</summary>
public static class MeritCycleSnapshotWorkflow
{
    public static MeritCycleSnapshotCoordinator NewMeritCycleSnapshotCoordinator(IMeritCycleSnapshotStore store, IClock clock)
    {
        return new MeritCycleSnapshotCoordinator(
            store,
            MeritCycleSnapshotRules.DefaultMeritCycleSnapshotValidator(),
            clock,
            new List<string>(),
            MeritCycleSnapshotEngine.DefaultMeritCycleSnapshotEngine());
    }
}
