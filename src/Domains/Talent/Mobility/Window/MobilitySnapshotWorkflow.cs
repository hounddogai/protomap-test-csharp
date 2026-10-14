namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves mobility snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record MobilitySnapshotCoordinator(IMobilitySnapshotStore Store, MobilitySnapshotValidator Validator, IClock Clock, List<string> Journal, MobilitySnapshotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MobilitySnapshotStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MobilitySnapshotStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility snapshot {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of mobility snapshot workflow.</summary>
public static class MobilitySnapshotWorkflow
{
    public static MobilitySnapshotCoordinator NewMobilitySnapshotCoordinator(IMobilitySnapshotStore store, IClock clock, Registry flags)
    {
        return new MobilitySnapshotCoordinator(
            store,
            MobilitySnapshotRules.DefaultMobilitySnapshotValidator(),
            clock,
            new List<string>(),
            MobilitySnapshotEngine.DefaultMobilitySnapshotEngine(flags));
    }
}
